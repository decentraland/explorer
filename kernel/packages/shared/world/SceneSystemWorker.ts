import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'

import { playerConfigurations } from 'config'
import { worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { SceneWorker } from './SceneWorker'
import { Vector3, Quaternion, Vector2 } from 'decentraland-ecs/src/decentraland/math'
import { PositionReport, positionObservable } from './positionThings'
import { Observer } from 'decentraland-ecs/src'
import { sceneLifeCycleObservable } from '../../decentraland-loader/lifecycle/controllers/scene'
import { renderStateObservable, isRendererEnabled } from './worldState'
import { ParcelSceneAPI } from './ParcelSceneAPI'
import { CustomWebWorkerTransport } from './CustomWebWorkerTransport'

const gamekitWorkerRaw = require('raw-loader!../../../static/systems/scene.system.js')
const gamekitWorkerBLOB = new Blob([gamekitWorkerRaw])
const gamekitWorkerUrl = URL.createObjectURL(gamekitWorkerBLOB)

const hudWorkerRaw = require('raw-loader!../../../static/systems/decentraland-ui.scene.js')
const hudWorkerBLOB = new Blob([hudWorkerRaw])
export const hudWorkerUrl = URL.createObjectURL(hudWorkerBLOB)

export class SceneSystemWorker extends SceneWorker {
  private sceneStarted: boolean = false

  private position: Vector3 = new Vector3()
  private readonly lastSentPosition = new Vector3(0, 0, 0)
  private readonly lastSentRotation = new Quaternion(0, 0, 0, 1)
  private positionObserver: Observer<any> | null = null
  private sceneLifeCycleObserver: Observer<any> | null = null
  private worldRunningObserver: Observer<any> | null = null

  private sceneReady: boolean = false

  constructor(
    parcelScene: ParcelSceneAPI,
    transport?: ScriptingTransport,
    private readonly persistent: boolean = false) {
    super(parcelScene, transport ?? SceneSystemWorker.buildWebWorkerTransport(parcelScene))

    this.subscribeToSceneLifeCycleEvents()
    this.subscribeToWorldRunningEvents()
    this.subscribeToPositionEvents()
  }

  private static buildWebWorkerTransport(parcelScene: ParcelSceneAPI): ScriptingTransport {
    const worker = new (Worker as any)(gamekitWorkerUrl, {
      name: `ParcelSceneWorker(${parcelScene.data.sceneId})`
    })
    // the first error handler will flag the error as a scene worker error enabling error
    // filtering in DCLUnityLoader.js, unhandled errors (like WebSocket messages failing)
    // are not handled by the update loop and therefore those break the whole worker
    const transportOverride = CustomWebWorkerTransport(worker)

    transportOverride.onError!((e: any) => {
      e['isSceneError'] = true
    })

    return transportOverride
  }

  setPosition(position: Vector3) {
    this.position = position
  }
  isPersistent(): boolean {
    return this.persistent
  }

  hasSceneStarted(): boolean {
    return this.sceneStarted
  }

  protected childDispose() {
    if (this.positionObserver) {
      positionObservable.remove(this.positionObserver)
      this.positionObserver = null
    }
    if (this.sceneLifeCycleObserver) {
      sceneLifeCycleObservable.remove(this.sceneLifeCycleObserver)
      this.sceneLifeCycleObserver = null
    }
    if (this.worldRunningObserver) {
      renderStateObservable.remove(this.worldRunningObserver)
      this.worldRunningObserver = null
    }
  }

  private sendUserViewMatrix(positionReport: Readonly<PositionReport>) {
    if (this.engineAPI && 'positionChanged' in this.engineAPI.subscribedEvents) {
      if (!this.lastSentPosition.equals(positionReport.position)) {
        this.engineAPI.sendSubscriptionEvent('positionChanged', {
          position: {
            x: positionReport.position.x - this.position.x,
            z: positionReport.position.z - this.position.z,
            y: positionReport.position.y
          },
          cameraPosition: positionReport.position,
          playerHeight: playerConfigurations.height
        })
        this.lastSentPosition.copyFrom(positionReport.position)
      }
    }

    if (this.engineAPI && 'rotationChanged' in this.engineAPI.subscribedEvents) {
      if (positionReport.quaternion && !this.lastSentRotation.equals(positionReport.quaternion)) {
        this.engineAPI.sendSubscriptionEvent('rotationChanged', {
          rotation: positionReport.rotation,
          quaternion: positionReport.quaternion
        })
        this.lastSentRotation.copyFrom(positionReport.quaternion)
      }
    }
  }

  private subscribeToPositionEvents() {
    const position = Vector2.Zero()

    this.positionObserver = positionObservable.add(obj => {
      worldToGrid(obj.position, position)

      this.sendUserViewMatrix(obj)
    })
  }

  private subscribeToWorldRunningEvents() {
    this.worldRunningObserver = renderStateObservable.add(() => {
      this.sendSceneReadyIfNecessary()
    })
  }

  private subscribeToSceneLifeCycleEvents() {
    this.sceneLifeCycleObserver = sceneLifeCycleObservable.add(obj => {
      if (this.getSceneId() === obj.sceneId && obj.status === 'ready') {
        this.sceneReady = true
        sceneLifeCycleObservable.remove(this.sceneLifeCycleObserver)
        this.sendSceneReadyIfNecessary()
      }
    })
  }

  private sendSceneReadyIfNecessary() {
    if (!this.sceneStarted && isRendererEnabled() && this.sceneReady) {
      this.sceneStarted = true
      this.engineAPI!.sendSubscriptionEvent('sceneStart', {})
      renderStateObservable.remove(this.worldRunningObserver)
    }
  }

}
