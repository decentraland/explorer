import { DEBUG, EDITOR, ENGINE_DEBUG_PANEL, NO_ASSET_BUNDLES, SCENE_DEBUG_PANEL, SHOW_FPS_COUNTER } from 'config'
import { loadingScenes, teleportTriggered } from 'shared/loading/types'
import { defaultLogger } from 'shared/logger'
import { ILand, LoadableParcelScene, MappingsResponse, SceneJsonData } from 'shared/types'
import {
  enableParcelSceneLoading,
  getParcelSceneID,
  loadParcelScene,
  stopParcelSceneWorker
} from 'shared/world/parcelSceneManager'
import { teleportObservable } from 'shared/world/positionThings'
import { SceneWorker } from 'shared/world/SceneWorker'
import { hudWorkerUrl } from 'shared/world/SceneSystemWorker'
import { observeLoadingStateChange, renderStateObservable } from 'shared/world/worldState'
import { ILandToLoadableParcelScene, ILandToLoadableParcelSceneUpdate } from 'shared/selectors'
import { UnityParcelScene } from './UnityParcelScene'
import { UnityInterface, unityInterface } from './UnityInterface'
import { clientDebug, ClientDebug } from './ClientDebug'
import { BrowserInterface } from './BrowserInterface'
import { UnityScene } from './UnityScene'
import { ensureUiApis } from 'shared/world/uiSceneInitializer'
import { WebSocketTransport } from 'decentraland-rpc'
import { kernelConfigForRenderer } from './kernelConfigForRenderer'
import type { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { store } from 'shared/store/store'

declare const globalThis: { clientDebug: ClientDebug }

export type RendererInterfaces = {
  unityInterface: UnityInterface
  browserInterface: BrowserInterface
}

globalThis.clientDebug = clientDebug

type GameInstance = {
  SendMessage(object: string, method: string, ...args: (number | string)[]): void
}

export let gameInstance!: GameInstance

function setLoadingScreenBasedOnState() {
  let state = store.getState()
  let loading = state?.loading

  unityInterface.SetLoadingScreen({
    isVisible: !loading.renderingActivated || loading?.showLoadingScreen || false,
    message: loading?.message || loading?.status || '',
    showWalletPrompt: false,
    showTips: loading?.initialLoad || false
  })
}

////////////////////////////////////////////////////////////////////////////////

function debuggingDecorator(_gameInstance: GameInstance) {
  const debug = false
  const decorator = {
    // @ts-ignore
    SendMessage: (...args) => {
      defaultLogger.info('gameInstance', ...args)
      // @ts-ignore
      _gameInstance.SendMessage(...args)
    }
  }
  return debug ? decorator : _gameInstance
}

/**
 *
 * Common initialization logic for the unity engine
 *
 * @param _gameInstance Unity game instance
 */
export async function initializeEngine(_gameInstance: GameInstance): Promise<void> {
  gameInstance = debuggingDecorator(_gameInstance)

  unityInterface.Init(_gameInstance)

  unityInterface.DeactivateRendering()

  unityInterface.SetKernelConfiguration(kernelConfigForRenderer())

  if (DEBUG) {
    unityInterface.SetDebug()
  }

  if (SCENE_DEBUG_PANEL) {
    unityInterface.SetSceneDebugPanel()
  }

  if (NO_ASSET_BUNDLES) {
    unityInterface.SetDisableAssetBundles()
  }

  if (SHOW_FPS_COUNTER) {
    unityInterface.ShowFPSPanel()
  }

  if (ENGINE_DEBUG_PANEL) {
    unityInterface.SetEngineDebugPanel()
  }

  observeLoadingStateChange(() => {
    setLoadingScreenBasedOnState()
  })

  renderStateObservable.add(() => {
    setLoadingScreenBasedOnState()
  })

  setLoadingScreenBasedOnState()

  if (!EDITOR) {
    await startGlobalScene(unityInterface, 'dcl-gs-avatars', 'Avatars', hudWorkerUrl)
  }
}

export async function startGlobalScene(
  unityInterface: UnityInterface,
  cid: string,
  title: string,
  fileContentUrl: string
) {
  const scene = new UnityScene({
    sceneId: cid,
    name: title,
    baseUrl: location.origin,
    main: fileContentUrl,
    useFPSThrottling: false,
    data: {},
    mappings: []
  })

  const worker = loadParcelScene(scene, undefined, true)

  await ensureUiApis(worker)

  unityInterface.CreateGlobalScene({
    id: getParcelSceneID(scene),
    name: scene.data.name,
    baseUrl: scene.data.baseUrl,
    isPortableExperience: false,
    contents: []
  })
}

export async function startUnitySceneWorkers() {
  store.dispatch(loadingScenes())

  await enableParcelSceneLoading({
    parcelSceneClass: UnityParcelScene,
    preloadScene: async (_land) => {
      // TODO:
      // 1) implement preload call
      // 2) await for preload message or timeout
      // 3) return
    },
    onLoadParcelScenes: (lands) => {
      unityInterface.LoadParcelScenes(
        lands.map(($) => {
          const x = Object.assign({}, ILandToLoadableParcelScene($).data)
          delete x.land
          return x
        })
      )
    },
    onUnloadParcelScenes: (lands) => {
      lands.forEach(($) => {
        unityInterface.UnloadScene($.sceneId)
      })
    },
    onPositionSettled: (spawnPoint) => {
      unityInterface.Teleport(spawnPoint)
      unityInterface.ActivateRendering()
    },
    onPositionUnsettled: () => {
      unityInterface.DeactivateRendering()
    }
  })
}

// Builder functions
let currentLoadedScene: SceneWorker | null

export async function loadPreviewScene(ws?: string): Promise<ILand> {
  const result = await fetch('/scene.json?nocache=' + Math.random())

  let lastId: string | null = null

  if (currentLoadedScene) {
    lastId = currentLoadedScene.getSceneId()
    stopParcelSceneWorker(currentLoadedScene)
  }

  if (result.ok) {
    // we load the scene to get the metadata
    // about rhe bounds and position of the scene
    // TODO(fmiras): Validate scene according to https://github.com/decentraland/proposals/blob/master/dsp/0020.mediawiki
    const scene = (await result.json()) as SceneJsonData
    const mappingsFetch = await fetch('/mappings')
    const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

    let defaultScene: ILand = {
      sceneId: 'previewScene',
      baseUrl: location.toString().replace(/\?[^\n]+/g, ''),
      baseUrlBundles: '',
      sceneJsonData: scene,
      mappingsResponse: mappingsResponse
    }

    const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(defaultScene))

    let transport: undefined | ScriptingTransport = undefined

    if (ws) {
      transport = WebSocketTransport(new WebSocket(ws, ['dcl-scene']))
    }

    currentLoadedScene = loadParcelScene(parcelScene, transport)

    const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(defaultScene).data }
    delete target.land

    defaultLogger.info('Reloading scene...')

    if (lastId) {
      unityInterface.UnloadScene(lastId)
    }

    unityInterface.LoadParcelScenes([target])

    defaultLogger.info('finish...')

    return defaultScene
  } else {
    throw new Error('Could not load scene.json')
  }
}

export function loadBuilderScene(sceneData: ILand) {
  unloadCurrentBuilderScene()

  const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(sceneData))
  currentLoadedScene = loadParcelScene(parcelScene)

  const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(sceneData).data }
  delete target.land

  unityInterface.LoadParcelScenes([target])
  return parcelScene
}

export function unloadCurrentBuilderScene() {
  if (currentLoadedScene) {
    unityInterface.DeactivateRendering()
    currentLoadedScene.emit('builderSceneUnloaded', {})

    stopParcelSceneWorker(currentLoadedScene)
    unityInterface.SendBuilderMessage('UnloadBuilderScene', currentLoadedScene.getSceneId())
    currentLoadedScene = null
  }
}

export function updateBuilderScene(sceneData: ILand) {
  if (currentLoadedScene) {
    const target: LoadableParcelScene = { ...ILandToLoadableParcelSceneUpdate(sceneData).data }
    delete target.land
    unityInterface.UpdateParcelScenes([target])
  }
}

teleportObservable.add((position: { x: number; y: number; text?: string }) => {
  // before setting the new position, show loading screen to avoid showing an empty world
  store.dispatch(teleportTriggered(position.text || `Teleporting to ${position.x}, ${position.y}`))
})

document.addEventListener('pointerlockchange', pointerLockChange, false)

let isPointerLocked: boolean = false

function pointerLockChange() {
  const doc: any = document
  const isLocked = (doc.pointerLockElement || doc.mozPointerLockElement || doc.webkitPointerLockElement) != null
  if (isPointerLocked !== isLocked) {
    unityInterface.SetCursorState(isLocked)
  }
  isPointerLocked = isLocked
}
