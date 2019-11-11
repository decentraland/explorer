// tslint:disable:no-console
declare var global: any & { isEditor: boolean; editor: any }
declare var window: Window & { isEditor: boolean }

global.isEditor = window.isEditor = true

import { EventEmitter } from 'events'
import future, { IFuture } from 'fp-future'

import { loadedSceneWorkers } from '../shared/world/parcelSceneManager'
import { IScene, normalizeContentMappings, ILand } from '../shared/types'
import { SceneWorker } from '../shared/world/SceneWorker'
import { initializeUnity } from '../unity-interface/initializer'
import {
  UnityParcelScene,
  loadBuilderScene,
  updateBuilderScene,
  futures,
  unloadCurrentBuilderScene,
  unityInterface
} from '../unity-interface/dcl'
import defaultLogger from '../shared/logger'
import { uuid } from '../decentraland-ecs/src/ecs/helpers'
import { Vector3 } from '../decentraland-ecs/src/decentraland/math'
import { sceneLifeCycleObservable } from '../decentraland-loader/lifecycle/controllers/scene'

const evtEmitter = new EventEmitter()
const initializedEngine = future<void>()

let unityScene: UnityParcelScene | undefined
let loadingEntities: string[] = []
let builderSceneLoaded: IFuture<boolean> = future()

/**
 * Function executed by builder
 * It creates the builder scene, binds the scene events and stubs the content mappings
 */
async function createBuilderScene(scene: IScene & { baseUrl: string }) {
  const isFirstRun = unityScene === undefined
  const sceneData = await getSceneData(scene)
  unityScene = loadBuilderScene(sceneData)
  bindSceneEvents()

  const engineReady = future()
  sceneLifeCycleObservable.addOnce(obj => {
    if (sceneData.sceneId === obj.sceneId && obj.status === 'ready') {
      engineReady.resolve(true)
    }
  })
  await engineReady

  if (isFirstRun) {
    unityInterface.SetBuilderReady()
  } else {
    unityInterface.ResetBuilderScene()
  }
  await builderSceneLoaded

  unityInterface.ActivateRendering()
  evtEmitter.emit('ready', {})
}

async function renewBuilderScene(scene: IScene & { baseUrl: string }) {
  if (unityScene) {
    scene.baseUrl = unityScene.data.baseUrl
    const sceneData = await getSceneData(scene)
    updateBuilderScene(sceneData)
  }
}

/**
 * It fakes the content mappings for being used at the Builder without
 * content server plus loads and creates the scene worker
 */
async function getSceneData(scene: IScene & { baseUrl: string }): Promise<ILand> {
  const id = getBaseCoords(scene)
  const publisher = '0x0'
  const contents = normalizeContentMappings(scene._mappings || [])

  if (!scene.baseUrl) {
    throw new Error('baseUrl missing in scene')
  }

  return {
    name: 'Editor scene',
    baseUrl: scene.baseUrl,
    sceneId: '0, 0',
    scene,
    mappingsResponse: {
      contents,
      parcel_id: id,
      publisher,
      root_cid: 'Qmtest'
    }
  }
}

/**
 * It returns base parcel if exists on `scene.json` or "0,0" if `baseParcel` missing
 */
function getBaseCoords(scene: IScene): string {
  if (scene && scene.scene && scene.scene.base) {
    const [x, y] = scene.scene.base.split(',').map($ => parseInt($, 10))
    return `${x},${y}`
  }

  return '0,0'
}

function bindSceneEvents() {
  if (!unityScene) return

  unityScene.on('uuidEvent' as any, event => {
    const { type } = event.payload

    if (type === 'onEntityLoading') {
      loadingEntities.push(event.payload.entityId)
    } else if (type === 'onEntityFinishLoading') {
      let index = loadingEntities.indexOf(event.payload.entityId)
      if (index >= 0) {
        loadingEntities.splice(index, 1)
      }
    }
  })

  unityScene.on('metricsUpdate', e => {
    evtEmitter.emit('metrics', {
      metrics: e.given,
      limits: e.limit
    })
  })

  unityScene.on('entitiesOutOfBoundaries', e => {
    evtEmitter.emit('entitiesOutOfBoundaries', e)
  })

  unityScene.on('entityOutOfScene', e => {
    evtEmitter.emit('entityOutOfScene', e)
  })

  unityScene.on('entityBackInScene', e => {
    evtEmitter.emit('entityBackInScene', e)
  })

  unityScene.on('builderSceneStart', e => {
    builderSceneLoaded.resolve(true)
  })

  unityScene.on('builderSceneUnloaded', e => {
    loadingEntities = []
  })
  unityScene.on('gizmoEvent', e => {
    if (e.type === 'gizmoSelected') {
      evtEmitter.emit('gizmoSelected', {
        gizmoType: e.gizmoType,
        entityId: e.entityId !== '' ? e.entityId : null
      })
    } else if (e.type === 'gizmoDragEnded') {
      evtEmitter.emit('transform', {
        entityId: e.entityId,
        transform: e.transform
      })
    }
  })
}

namespace editor {
  /**
   * Function executed by builder which is the first function of the entry point
   */
  export async function initEngine(container: HTMLElement, buildConfigPath: string) {
    try {
      await initializeUnity(container, buildConfigPath)
      defaultLogger.log('Engine initialized.')
      initializedEngine.resolve()
    } catch (err) {
      defaultLogger.error('Error loading Unity', err)
      initializedEngine.reject(err)
      throw err
    }
  }

  export async function handleMessage(message: any) {
    if (message.type === 'update') {
      await initializedEngine
      await createBuilderScene(message.payload.scene)
    }
  }

  export function setGridResolution(position: number, rotation: number, scale: number) {
    unityInterface.SetBuilderGridResolution(position, rotation, scale)
  }

  export function selectEntity(entityId: string) {
    unityInterface.SelectBuilderEntity(entityId)
  }

  export function deselectEntity() {
    unityInterface.DeselectBuilderEntity()
  }

  export function getDCLCanvas() {
    return document.getElementById('#canvas')
  }

  export function getScenes(): Set<SceneWorker> {
    return new Set(loadedSceneWorkers.values())
  }

  export async function sendExternalAction(action: { type: string; payload: { [key: string]: any } }) {
    if (action.type === 'Close editor') {
      unloadCurrentBuilderScene()
      unityInterface.DeactivateRendering()
    } else if (unityScene) {
      const { worker } = unityScene
      if (action.payload.mappings) {
        const scene = { ...action.payload.scene }
        scene._mappings = action.payload.mappings
        await renewBuilderScene(scene)
      }
      worker.engineAPI!.sendSubscriptionEvent('externalAction', action)
    }
  }

  export function selectGizmo(type: string) {
    unityInterface.SelectGizmoBuilder(type)
  }

  export async function setPlayMode(on: boolean) {
    const onString: string = on ? 'true' : 'false'
    unityInterface.SetPlayModeBuilder(onString)
  }

  export function on(evt: string, listener: (...args: any[]) => void) {
    evtEmitter.addListener(evt, listener)
  }

  export function off(evt: string, listener: (...args: any[]) => void) {
    evtEmitter.removeListener(evt, listener)
  }

  export function setCameraZoomDelta(delta: number) {
    unityInterface.SetCameraZoomDeltaBuilder(delta)
  }

  export function getCameraTarget() {
    const id = uuid()
    futures[id] = future()
    unityInterface.GetCameraTargetBuilder(id)
    return futures[id]
  }

  export function resetCameraZoom() {
    unityInterface.ResetCameraZoomBuilder()
  }

  export function getMouseWorldPosition(x: number, y: number): IFuture<Vector3> {
    const id = uuid()
    futures[id] = future()
    unityInterface.GetMousePositionBuilder(x.toString(), y.toString(), id)
    return futures[id]
  }

  export function handleUnitySomeVale(id: string, value: Vector3) {
    futures[id].resolve(value)
  }

  export function preloadFile(url: string) {
    unityInterface.PreloadFileBuilder(url)
  }

  export function setCameraRotation(alpha: number, beta: number) {
    unityInterface.SetCameraRotationBuilder(alpha, beta)
  }

  export function getLoadingEntity() {
    if (loadingEntities.length === 0) {
      return null
    } else {
      return loadingEntities[0]
    }
  }

  export function takeScreenshot(mime?: string): IFuture<string> {
    const id = uuid()
    futures[id] = future()
    unityInterface.TakeScreenshotBuilder(id)
    return futures[id]
  }

  export function setCameraPosition(position: Vector3) {
    unityInterface.SetCameraPositionBuilder(position)
  }

  export function onKeyDown(key: string) {
    unityInterface.OnBuilderKeyDown(key)
  }
}

global.editor = editor
