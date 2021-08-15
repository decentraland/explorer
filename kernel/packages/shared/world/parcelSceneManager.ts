import { initParcelSceneWorker } from 'decentraland-loader/lifecycle/manager'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import {
  sceneLifeCycleObservable,
  renderDistanceObservable
} from '../../decentraland-loader/lifecycle/controllers/scene'
import { trackEvent } from '../analytics'
import { informPendingScenes, signalSceneFail, signalSceneLoad, signalSceneStart } from '../loading/actions'
import { EnvironmentData, ILand, InstancedSpawnPoint, LoadableParcelScene } from '../types'
import { ParcelSceneAPI } from './ParcelSceneAPI'
import { parcelObservable, teleportObservable } from './positionThings'
import { SceneWorker, SceneWorkerReadyState } from './SceneWorker'
import { SceneSystemWorker } from './SceneSystemWorker'
import { ILandToLoadableParcelScene } from 'shared/selectors'
import { store } from 'shared/store/isolatedStore'

export type EnableParcelSceneLoadingOptions = {
  parcelSceneClass: { new (x: EnvironmentData<LoadableParcelScene>): ParcelSceneAPI }
  preloadScene: (parcelToLoad: ILand) => Promise<any>
  onPositionSettled?: (spawnPoint: InstancedSpawnPoint) => void
  onLoadParcelScenes?(x: ILand[]): void
  onUnloadParcelScenes?(x: ILand[]): void
  onPositionUnsettled?(): void
}

declare const globalThis: any

export const loadedSceneWorkers = new Map<string, SceneWorker>()
globalThis['sceneWorkers'] = loadedSceneWorkers

/**
 * Retrieve the Scene based on it's ID, usually RootCID
 */
export function getSceneWorkerBySceneID(sceneId: string) {
  return loadedSceneWorkers.get(sceneId)
}

/**
 * Returns the id of the scene, usually the RootCID
 */
export function getParcelSceneID(parcelScene: ParcelSceneAPI) {
  return parcelScene.data.sceneId
}

/** Stops non-persistent scenes (i.e UI scene) */
export function stopParcelSceneWorker(worker: SceneWorker) {
  if (worker && !worker.isPersistent()) {
    forceStopParcelSceneWorker(worker)
  }
}

export function forceStopParcelSceneWorker(worker: SceneWorker) {
  const sceneId = worker.getSceneId()
  worker.dispose()
  loadedSceneWorkers.delete(sceneId)
  reportPendingScenes()
}

export function loadParcelScene(
  parcelScene: ParcelSceneAPI,
  transport?: ScriptingTransport,
  persistent: boolean = false
) {
  const sceneId = getParcelSceneID(parcelScene)

  let parcelSceneWorker = loadedSceneWorkers.get(sceneId)

  if (!parcelSceneWorker) {
    parcelSceneWorker = new SceneSystemWorker(parcelScene, transport, persistent)

    setNewParcelScene(sceneId, parcelSceneWorker)
  }

  return parcelSceneWorker
}

export function setNewParcelScene(sceneId: string, worker: SceneWorker) {
  let parcelSceneWorker = loadedSceneWorkers.get(sceneId)

  if (parcelSceneWorker) {
    forceStopParcelSceneWorker(parcelSceneWorker)
  }

  loadedSceneWorkers.set(sceneId, worker)
  globalSignalSceneLoad(sceneId)
}

function globalSignalSceneLoad(sceneId: string) {
  store.dispatch(signalSceneLoad(sceneId))
  reportPendingScenes()
}

function globalSignalSceneStart(sceneId: string) {
  store.dispatch(signalSceneStart(sceneId))
  reportPendingScenes()
}

function globalSignalSceneFail(sceneId: string) {
  store.dispatch(signalSceneFail(sceneId))
  reportPendingScenes()
}

function reportPendingScenes() {
  const pendingScenes = new Set<string>()

  let countableScenes = 0
  for (let [sceneId, sceneWorker] of loadedSceneWorkers) {
    // avatar scene should not be counted here
    const shouldBeCounted = !sceneWorker.isPersistent()

    const isPending = (sceneWorker.ready & SceneWorkerReadyState.STARTED) === 0
    const failedLoading = (sceneWorker.ready & SceneWorkerReadyState.LOADING_FAILED) !== 0
    if (shouldBeCounted) {
      countableScenes++
    }
    if (shouldBeCounted && isPending && !failedLoading) {
      pendingScenes.add(sceneId)
    }
  }

  store.dispatch(informPendingScenes(pendingScenes.size, countableScenes))
}

export async function enableParcelSceneLoading(options: EnableParcelSceneLoadingOptions) {
  const ret = await initParcelSceneWorker()

  ret.on('Scene.shouldPrefetch', async (opts: { sceneId: string }) => {
    const parcelSceneToLoad = await ret.getParcelData(opts.sceneId)

    // start and await prefetch
    await options.preloadScene(parcelSceneToLoad)

    // continue with the loading
    ret.notify('Scene.prefetchDone', opts)
  })

  ret.on('Scene.shouldStart', async (opts: { sceneId: string }) => {
    const sceneId = opts.sceneId
    const parcelSceneToStart = await ret.getParcelData(sceneId)

    // create the worker if don't exist
    if (!getSceneWorkerBySceneID(sceneId)) {
      const parcelScene = new options.parcelSceneClass(ILandToLoadableParcelScene(parcelSceneToStart))
      parcelScene.data.useFPSThrottling = true
      loadParcelScene(parcelScene)
    }

    let timer: ReturnType<typeof setTimeout>

    const observer = sceneLifeCycleObservable.add((sceneStatus) => {
      const worker = getSceneWorkerBySceneID(sceneId)
      if (worker && sceneStatus.sceneId === sceneId && (worker.ready & SceneWorkerReadyState.STARTED) === 0) {
        sceneLifeCycleObservable.remove(observer)
        clearTimeout(timer)
        worker.ready |= SceneWorkerReadyState.STARTED
        ret.notify('Scene.status', sceneStatus)
        globalSignalSceneStart(sceneId)
      }
    })

    // tell the engine to load the parcel scene
    if (options.onLoadParcelScenes) {
      options.onLoadParcelScenes([parcelSceneToStart])
    }

    timer = setTimeout(() => {
      const worker = getSceneWorkerBySceneID(sceneId)
      if (worker && !worker.hasSceneStarted()) {
        sceneLifeCycleObservable.remove(observer)
        worker.ready |= SceneWorkerReadyState.LOADING_FAILED
        ret.notify('Scene.status', { sceneId, status: 'failed' })
        globalSignalSceneFail(sceneId)
      }
    }, 90000)
  })

  ret.on('Scene.shouldUnload', async (opts: { sceneId: string }) => {
    const worker = loadedSceneWorkers.get(opts.sceneId)
    if (!worker) {
      return
    }
    stopParcelSceneWorker(worker)
    if (options.onUnloadParcelScenes) {
      options.onUnloadParcelScenes([await ret.getParcelData(opts.sceneId)])
    }
  })

  ret.on('Position.settled', async (opts: { spawnPoint: InstancedSpawnPoint }) => {
    if (options.onPositionSettled) {
      options.onPositionSettled(opts.spawnPoint)
    }
  })

  ret.on('Position.unsettled', () => {
    if (options.onPositionUnsettled) {
      options.onPositionUnsettled()
    }
  })

  ret.on('Event.track', (event: { name: string; data: any }) => {
    trackEvent(event.name, event.data)
  })

  teleportObservable.add((position: { x: number; y: number }) => {
    ret.notify('User.setPosition', { position, teleported: true })
  })

  renderDistanceObservable.add((event) => {
    ret.notify('SetScenesLoadRadius', event)
  })

  parcelObservable.add((obj) => {
    // immediate reposition should only be broadcasted to others, otherwise our scene reloads
    if (obj.immediate) return

    ret.notify('User.setPosition', { position: obj.newParcel, teleported: false })
  })
}
