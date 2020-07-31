import { EventDispatcher } from 'decentraland-rpc/lib/common/core/EventDispatcher'
import { WSS_ENABLED } from 'config'
import { IEventNames, IEvents } from '../decentraland-ecs/src/decentraland/Types'
import { createLogger, ILogger } from 'shared/logger'
import { EntityAction, EnvironmentData } from 'shared/types'
import { ParcelSceneAPI } from 'shared/world/ParcelSceneAPI'
import { getParcelSceneID } from 'shared/world/parcelSceneManager'
import { SceneWorker } from 'shared/world/SceneWorker'
import { unityInterface } from './UnityInterface'
import { protobufMsgBridge } from './protobufMessagesBridge'
import { nativeMsgBridge } from './nativeMessagesBridge'

export class UnityScene<T> implements ParcelSceneAPI {
  eventDispatcher = new EventDispatcher()
  worker!: SceneWorker
  logger: ILogger

  constructor(public data: EnvironmentData<T>) {
    this.logger = createLogger(getParcelSceneID(this) + ': ')
  }

  sendBatch(actions: EntityAction[]): void {
    if (WSS_ENABLED) {
      this.sendBatchWss(unityInterface, actions)
    } else {
      this.sendBatchNative(unityInterface, actions)
    }
  }

  sendBatchWss(unityInterface: any, actions: EntityAction[]): void {
    const sceneId = getParcelSceneID(this)
    let messages = ''
    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]
      messages += protobufMsgBridge.encodeSceneMessage(sceneId, action.type, action.payload, action.tag)
      messages += '\n'
    }

    unityInterface.SendSceneMessage(messages)
  }

  sendBatchNative(unityInterface: any, actions: EntityAction[]): void {
    const sceneId = getParcelSceneID(this)
    let messages = ''
    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]
      if (nativeMsgBridge.isMethodSupported(action.type)) {
        if (messages.length > 0) {
          unityInterface.SendSceneMessage(messages)
          messages = ''
        }
        nativeMsgBridge.SendNativeMessage(sceneId, action)
        continue
      }

      messages += protobufMsgBridge.encodeSceneMessage(sceneId, action.type, action.payload, action.tag)
      messages += '\n'
    }

    if (messages.length > 0) {
      unityInterface.SendSceneMessage(messages)
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
