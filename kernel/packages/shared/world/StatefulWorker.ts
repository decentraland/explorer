import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { ParcelSceneAPI } from './ParcelSceneAPI'
import { SceneWorker } from './SceneWorker'
import { CustomWebWorkerTransport } from './CustomWebWorkerTransport'
import { SceneStateStorageController } from 'shared/apis/SceneStateStorageController/SceneStateStorageController'
import { defaultLogger } from 'shared/logger'

const gamekitWorkerRaw = require('raw-loader!../../../static/systems/stateful.scene.system.js')
const gamekitWorkerBLOB = new Blob([gamekitWorkerRaw])
const gamekitWorkerUrl = URL.createObjectURL(gamekitWorkerBLOB)

export class StatefulWorker extends SceneWorker {
  constructor(parcelScene: ParcelSceneAPI) {
    super(parcelScene, StatefulWorker.buildWebWorkerTransport(parcelScene))

    const klass = SceneStateStorageController
    Object.defineProperty(klass, 'name', SceneStateStorageController)
    this.getAPIInstance(klass).catch((error) =>
      defaultLogger.error('Failed to load the SceneStateStorageController', error)
    )
  }

  private static buildWebWorkerTransport(parcelScene: ParcelSceneAPI): ScriptingTransport {
    const worker = new (Worker as any)(gamekitWorkerUrl, {
      name: `StatefulWorker(${parcelScene.data.sceneId})`
    })

    return CustomWebWorkerTransport(worker)
  }

  setPosition() {
    return
  }

  isPersistent(): boolean {
    return false
  }

  hasSceneStarted(): boolean {
    return true
  }

  protected childDispose() {
    return
  }
}
