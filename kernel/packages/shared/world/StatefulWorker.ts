import { future } from 'fp-future'
import { APIOptions, ScriptingHost } from 'decentraland-rpc/lib/host'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { WebWorkerTransport } from 'decentraland-rpc'

import { defaultLogger } from 'shared/logger'
import { EnvironmentAPI } from 'shared/apis/EnvironmentAPI'
import { IEventNames, IEvents } from 'decentraland-ecs/src'
import { ParcelSceneAPI } from './ParcelSceneAPI'
import { getParcelSceneID } from './parcelSceneManager'
import { UnityParcelScene } from 'unity-interface/UnityParcelScene'
import { SceneWorker } from './SceneWorker'

// tslint:disable-next-line:whitespace
type EngineAPI = import('../apis/EngineAPI').EngineAPI

const gamekitWorkerRaw = require('raw-loader!../../../static/systems/stateful.scene.system.js')
const gamekitWorkerBLOB = new Blob([gamekitWorkerRaw])
const gamekitWorkerUrl = URL.createObjectURL(gamekitWorkerBLOB)

// this function is used in a onSystemReady.then(unmountSystem).
// we keep it separated and global because it is highly reusable
function unmountSystem(system: ScriptingHost) {
  try {
    system.unmount()
  } catch (e) {
    defaultLogger.error('Error unmounting system', e)
  }
}

export class StatefulWorker implements SceneWorker {
  private readonly system = future<ScriptingHost>()
  private engineAPI: EngineAPI | null = null
  private enabled = true

  constructor(private readonly parcelScene: ParcelSceneAPI) {
    parcelScene.registerWorker(this)

    this.loadSystem()
      .then($ => this.system.resolve($))
      .catch($ => this.system.reject($))
  }

  getParcelScene(): ParcelSceneAPI {
    return this.parcelScene
  }

  dispose() {
    if (this.enabled) {
      this.enabled = false

      // Unmount the system
      if (this.system) {
        this.system.then(unmountSystem).catch(e => defaultLogger.error('Unable to unmount system', e))
      }

      this.parcelScene.dispose()
    }
  }

  setPosition() { }

  sendSubscriptionEvent() { }

  emit<T extends IEventNames>(event: T, data: IEvents[T]): void {
    if (this.parcelScene instanceof UnityParcelScene) {
      this.parcelScene.emit(event, data)
    }
  }

  isPersistent(): boolean {
    return false
  }

  hasSceneStarted(): boolean {
    return true
  }

  getSceneId(): string {
    return getParcelSceneID(this.parcelScene)
  }

  getAPIInstance<X>(api: { new(options: APIOptions): X }): Promise<X> {
    return this.system.then(system => system.getAPIInstance(api))
  }

  private async startSystem(transport: ScriptingTransport) {
    const system = await ScriptingHost.fromTransport(transport)

    this.engineAPI = system.getAPIInstance('EngineAPI') as EngineAPI
    this.engineAPI.parcelSceneAPI = this.parcelScene

    system.getAPIInstance(EnvironmentAPI).data = this.parcelScene.data

    system.enable()

    return system
  }

  private async loadSystem(): Promise<ScriptingHost> {
    const worker = new (Worker as any)(gamekitWorkerUrl, {
      name: `StatefulWorker(${getParcelSceneID(this.parcelScene)})`
    })
    return this.startSystem(WebWorkerTransport(worker))
  }
}
