import { EventDispatcher } from 'decentraland-rpc/lib/common/core/EventDispatcher'
import { WSS_ENABLED, FORCE_SEND_MESSAGE, DEBUG_MESSAGES_QUEUE_PERF, DEBUG_SCENE_LOG } from 'config'
import type { IEventNames, IEvents } from 'decentraland-ecs'
import { createLogger, ILogger, createDummyLogger } from 'shared/logger'
import { EntityAction, EnvironmentData } from 'shared/types'
import { ParcelSceneAPI } from 'shared/world/ParcelSceneAPI'
import { SceneWorker } from 'shared/world/SceneWorker'
import { getUnityInstance } from './IUnityInterface'
import { protobufMsgBridge } from './protobufMessagesBridge'
import { nativeMsgBridge } from './nativeMessagesBridge'
import { trackEvent } from 'shared/analytics'

/**
 * Returns the id of the scene, usually the RootCID
 */
export function getParcelSceneID(parcelScene: ParcelSceneAPI) {
  return parcelScene.data.sceneId
}

let sendBatchTime: Array<number> = []
let sendBatchMsgs: Array<number> = []
let sendBatchTimeCount: number = 0
let sendBatchMsgCount: number = 0

export class UnityScene<T> implements ParcelSceneAPI {
  eventDispatcher = new EventDispatcher()
  worker!: SceneWorker
  logger: ILogger
  initMessageCount: number = 0
  initFinished: boolean = false

  constructor(public data: EnvironmentData<T>) {
    this.logger = DEBUG_SCENE_LOG === true ? createLogger(getParcelSceneID(this) + ': ') : createDummyLogger()

    const startLoadingTime = performance.now()

    this.eventDispatcher.once('sceneStart', () => {
      trackEvent('scene_start_event', {
        scene_id: getParcelSceneID(this),
        time_since_creation: performance.now() - startLoadingTime
      })
    })
  }

  sendBatch(actions: EntityAction[]): void {
    let time = Date.now()
    if (WSS_ENABLED || FORCE_SEND_MESSAGE) {
      this.sendBatchWss(actions)
    } else {
      this.sendBatchNative(actions)
    }

    if (DEBUG_MESSAGES_QUEUE_PERF) {
      time = Date.now() - time

      sendBatchTime.push(time)
      sendBatchMsgs.push(actions.length)

      sendBatchTimeCount += time

      sendBatchMsgCount += actions.length

      while (sendBatchMsgCount >= 10000) {
        sendBatchTimeCount -= sendBatchTime.splice(0, 1)[0]
        sendBatchMsgCount -= sendBatchMsgs.splice(0, 1)[0]
      }

      // tslint:disable-next-line:no-console
      console.log(`sendBatch time total for msgs ${sendBatchMsgCount} calls: ${sendBatchTimeCount}ms ... `)
    }
  }

  sendBatchWss(actions: EntityAction[]): void {
    const sceneId = getParcelSceneID(this)
    let messages = ''
    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]

      // Check moved from SceneRuntime.ts->DecentralandInterface.componentUpdate() here until we remove base64 support.
      // This way we can still initialize problematic scenes in the Editor, otherwise the protobuf encoding explodes with such messages.
      if (action.payload.json?.length > 49000) {
        this.logger.error('Component payload cannot exceed 49.000 bytes. Skipping message.')

        continue
      }

      messages += protobufMsgBridge.encodeSceneMessage(sceneId, action.type, action.payload, action.tag)
      messages += '\n'
    }

    getUnityInstance().SendSceneMessage(messages)
  }

  sendBatchNative(actions: EntityAction[]): void {
    const sceneId = getParcelSceneID(this)
    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]
      nativeMsgBridge.SendNativeMessage(sceneId, action)
    }
  }

  registerWorker(worker: SceneWorker): void {
    this.worker = worker
  }

  dispose(): void {
    // TODO: do we need to release some resource after releasing a scene worker?
  }

  on<T extends IEventNames>(event: T, cb: (event: IEvents[T]) => void): void {
    this.eventDispatcher.on(event, cb)
  }

  emit<T extends IEventNames>(event: T, data: IEvents[T]): void {
    this.eventDispatcher.emit(event, data)
  }
}
