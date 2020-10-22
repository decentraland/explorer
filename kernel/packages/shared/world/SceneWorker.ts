import { future } from 'fp-future'
import { APIOptions, ScriptingHost } from 'decentraland-rpc/lib/host'
import { ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { WebWorkerTransport } from 'decentraland-rpc'

import { playerConfigurations } from 'config'
import { worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { defaultLogger } from 'shared/logger'
import { EnvironmentAPI } from 'shared/apis/EnvironmentAPI'
import { Vector3, Quaternion, Vector2 } from 'decentraland-ecs/src/decentraland/math'
import { PositionReport, positionObservable } from './positionThings'
import { IEventNames, IEvents, Observer } from 'decentraland-ecs/src'
import { sceneLifeCycleObservable } from '../../decentraland-loader/lifecycle/controllers/scene'
import { worldRunningObservable, isWorldRunning } from './worldState'
import { ParcelSceneAPI } from './ParcelSceneAPI'
import { getParcelSceneID } from './parcelSceneManager'
import { UnityParcelScene } from 'unity-interface/UnityParcelScene'

// tslint:disable-next-line:whitespace
type EngineAPI = import('../apis/EngineAPI').EngineAPI

const gamekitWorkerRaw = require('raw-loader!../../../static/systems/scene.system.js')
const gamekitWorkerBLOB = new Blob([gamekitWorkerRaw])
const gamekitWorkerUrl = URL.createObjectURL(gamekitWorkerBLOB)

const hudWorkerRaw = require('raw-loader!../../../static/systems/decentraland-ui.scene.js')
const hudWorkerBLOB = new Blob([hudWorkerRaw])
export const hudWorkerUrl = URL.createObjectURL(hudWorkerBLOB)

// this function is used in a onSystemReady.then(unmountSystem).
// we keep it separated and global because it is highly reusable
function unmountSystem(system: ScriptingHost) {
  try {
    system.unmount()
  } catch (e) {
    defaultLogger.error('Error unmounting system', e)
  }
}

export interface SceneWorker {
  getSceneId(): string
  getParcelScene(): ParcelSceneAPI
  dispose(): void
  setPosition(position: Vector3): void
  sendSubscriptionEvent<K extends IEventNames>(event: K, data: IEvents[K]): void
  emit<T extends IEventNames>(event: T, data: IEvents[T]): void
  isPersistent(): boolean
  hasSceneStarted(): boolean
  getAPIInstance<X>(api: { new(options: APIOptions): X }): Promise<X>
}

export class SceneSystemWorker implements SceneWorker {
  private readonly system = future<ScriptingHost>()

  private engineAPI: EngineAPI | null = null
  private enabled = true
  private sceneStarted: boolean = false

  private position: Vector3 = new Vector3()
  private readonly lastSentPosition = new Vector3(0, 0, 0)
  private readonly lastSentRotation = new Quaternion(0, 0, 0, 1)
  private positionObserver: Observer<any> | null = null
  private sceneLifeCycleObserver: Observer<any> | null = null
  private worldRunningObserver: Observer<any> | null = null

  private sceneReady: boolean = false

  constructor(
    private readonly parcelScene: ParcelSceneAPI,
    transport?: ScriptingTransport,
    private readonly persistent: boolean = false) {
    parcelScene.registerWorker(this)

    this.subscribeToSceneLifeCycleEvents()
    this.subscribeToWorldRunningEvents()

    this.loadSystem(transport)
      .then($ => this.system.resolve($))
      .catch($ => this.system.reject($))

    console.log(parcelScene.data.sceneId)
  }

  getParcelScene(): ParcelSceneAPI {
    return this.parcelScene
  }

  dispose() {
    if (this.enabled) {
      if (this.positionObserver) {
        positionObservable.remove(this.positionObserver)
        this.positionObserver = null
      }
      if (this.sceneLifeCycleObserver) {
        sceneLifeCycleObservable.remove(this.sceneLifeCycleObserver)
        this.sceneLifeCycleObserver = null
      }
      if (this.worldRunningObserver) {
        worldRunningObservable.remove(this.worldRunningObserver)
        this.worldRunningObserver = null
      }

      this.enabled = false

      // Unmount the system
      if (this.system) {
        this.system.then(unmountSystem).catch(e => defaultLogger.error('Unable to unmount system', e))
      }

      this.parcelScene.dispose()
    }
  }

  setPosition(position: Vector3) {
    this.position = position
  }

  sendSubscriptionEvent<K extends IEventNames>(event: K, data: IEvents[K]) {
    this.engineAPI?.sendSubscriptionEvent(event, data)
  }

  emit<T extends IEventNames>(event: T, data: IEvents[T]): void {
    if (this.parcelScene instanceof UnityParcelScene) {
      this.parcelScene.emit(event, data)
    }
  }

  isPersistent(): boolean {
    return this.persistent
  }

  hasSceneStarted(): boolean {
    return this.sceneStarted
  }

  getSceneId(): string {
    return getParcelSceneID(this.parcelScene)
  }

  getAPIInstance<X>(api: { new(options: APIOptions): X }): Promise<X> {
    return this.system.then(system => system.getAPIInstance(api))
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
    this.worldRunningObserver = worldRunningObservable.add(isRunning => {
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
    if (!this.sceneStarted && isWorldRunning() && this.sceneReady) {
      this.sceneStarted = true
      this.engineAPI!.sendSubscriptionEvent('sceneStart', {})
      worldRunningObservable.remove(this.worldRunningObserver)
    }
  }

  private async startSystem(transport: ScriptingTransport) {
    const system = await ScriptingHost.fromTransport(transport)

    this.engineAPI = system.getAPIInstance('EngineAPI') as EngineAPI
    this.engineAPI.parcelSceneAPI = this.parcelScene

    system.getAPIInstance(EnvironmentAPI).data = this.parcelScene.data

    system.enable()

    this.subscribeToPositionEvents()

    return system
  }

  private async loadSystem(transport?: ScriptingTransport): Promise<ScriptingHost> {
    if (transport) {
      return this.startSystem(transport)
    } else {
      const worker = new (Worker as any)(gamekitWorkerUrl, {
        name: `ParcelSceneWorker(${getParcelSceneID(this.parcelScene)})`
      })
      return this.startSystem(WebWorkerTransport(worker))
    }
  }
}
