import { future } from 'fp-future'
import { APIOptions, ScriptingHost } from 'decentraland-rpc/lib/host'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { defaultLogger } from 'shared/logger'
import { EnvironmentAPI } from 'shared/apis/EnvironmentAPI'
import { EngineAPI } from 'shared/apis/EngineAPI'
import { Vector3 } from 'decentraland-ecs'
import type { IEventNames, IEvents } from 'decentraland-ecs'
import { PREVIEW } from 'config'
import { ParcelSceneAPI } from './ParcelSceneAPI'

export enum SceneWorkerReadyState {
  LOADING = 1 << 0,
  LOADED = 1 << 1,
  STARTED = 1 << 2,
  LOADING_FAILED = 1 << 4,
  SYSTEM_FAILED = 1 << 5,
  DISPOSING = 1 << 6,
  SYSTEM_DISPOSED = 1 << 7,
  DISPOSED = 1 << 8
}

export abstract class SceneWorker {
  public ready: SceneWorkerReadyState = SceneWorkerReadyState.LOADING
  protected engineAPI: EngineAPI | null = null
  private readonly system = future<ScriptingHost>()

  constructor(private readonly parcelScene: ParcelSceneAPI, transport: ScriptingTransport) {
    parcelScene.registerWorker(this)

    this.startSystem(transport)
      .then(($) => this.system.resolve($))
      .catch(($) => this.system.reject($))
  }

  abstract setPosition(position: Vector3): void
  abstract isPersistent(): boolean
  abstract hasSceneStarted(): boolean

  getSceneId(): string {
    return this.parcelScene.data.sceneId
  }

  getParcelScene(): ParcelSceneAPI {
    return this.parcelScene
  }

  emit<T extends IEventNames>(event: T, data: IEvents[T]): void {
    this.parcelScene.emit(event, data)
  }

  getAPIInstance<X>(api: { new (options: APIOptions): X }): Promise<X> {
    return this.system.then((system) => system.getAPIInstance(api))
  }

  sendSubscriptionEvent<K extends IEventNames>(event: K, data: IEvents[K]) {
    this.engineAPI?.sendSubscriptionEvent(event, data)
  }

  dispose() {
    const disposingFlags =
      SceneWorkerReadyState.DISPOSING | SceneWorkerReadyState.SYSTEM_DISPOSED | SceneWorkerReadyState.DISPOSED

    if ((this.ready & disposingFlags) === 0) {
      this.ready |= SceneWorkerReadyState.DISPOSING
      this.childDispose()

      // Unmount the system
      this.system
        .then((system) => {
          try {
            system.unmount()
          } catch (e) {
            defaultLogger.error('Error unmounting system', e)
          }
          this.ready |= SceneWorkerReadyState.SYSTEM_DISPOSED
        })
        .catch((e) => {
          defaultLogger.error('Unable to unmount system', e)
          this.ready |= SceneWorkerReadyState.SYSTEM_DISPOSED
        })

      this.parcelScene.dispose()
      this.ready |= SceneWorkerReadyState.DISPOSED
    }
  }

  protected abstract childDispose(): void

  private async startSystem(transport: ScriptingTransport) {
    const system = await ScriptingHost.fromTransport(transport)

    this.engineAPI = system.getAPIInstance('EngineAPI') as EngineAPI
    this.engineAPI.parcelSceneAPI = this.parcelScene

    system.getAPIInstance(EnvironmentAPI).data = this.parcelScene.data

    // TODO: track this errors using rollbar because this kind of event are usually triggered due to setInterval() or unreliable code in scenes, that is not sandboxed
    system.on('error', (e) => {
      // @ts-ignore
      console['log']('Unloading scene because of unhandled exception in the scene worker: ')

      // @ts-ignore
      console['error'](e)

      // These errors should be handled in development time
      if (PREVIEW) {
        eval('debu' + 'gger')
      }

      transport.close()

      this.ready |= SceneWorkerReadyState.SYSTEM_FAILED
    })

    system.enable()

    this.ready |= SceneWorkerReadyState.LOADED

    return system
  }
}
