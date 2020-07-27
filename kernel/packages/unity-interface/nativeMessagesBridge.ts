import { CreateEntityPayload, RemoveEntityPayload, EntityAction } from 'shared/types'

export class NativeMessagesBridge {
  private __createEntity!: (sceneId: string, entityId: string) => void
  private __removeEntity!: (sceneId: string, entityId: string) => void
  private __setSceneStarted!: (sceneId: string) => void

  private currentSceneId: string = ''

  //private currentTag: number = 0

  public initNativeMessages() {
    let unityModule: any = (globalThis as any).Module

    if ( !unityModule ) {
      console.error("Unity module not found! Are you in WSS mode?")
      return
    }

    this.__createEntity = unityModule.cwrap('call_create_entity', null, ['string', 'string'])
    this.__removeEntity = unityModule.cwrap('call_remove_entity', null, ['string', 'string'])
    this.__setSceneStarted = unityModule.cwrap('call_scene_started', null, ['string'])
  }

  public optimizeSendMessage() {
    //no-op
  }

  public isMethodSupported(method: string): boolean {
    return method === 'CreateEntity' || method === 'RemoveEntity' || method === 'InitMessagesFinished'
  }

  public setSceneId(sceneId: string) {
    this.currentSceneId = sceneId
  }

  public setTag(tag: string) {
    //this.currentTag = tag
  }

  public createEntity(payload: CreateEntityPayload) {
    this.__createEntity(this.currentSceneId, payload.id)
  }

  public removeEntity(payload: RemoveEntityPayload) {
    this.__removeEntity(this.currentSceneId, payload.id)
  }

  public setSceneStarted() {
    this.__setSceneStarted(this.currentSceneId)
  }

  public SendNativeMessage(parcelSceneId: string, action: EntityAction): void {
    this.setSceneId(parcelSceneId)

    if (action.tag !== undefined) this.setTag(action.tag)

    switch (action.type) {
      case 'CreateEntity':
        this.createEntity(action.payload)
        break
      case 'RemoveEntity':
        this.removeEntity(action.payload)
        break
      case 'InitMessagesFinished':
        this.setSceneStarted()
        break
    }
  }
}

// tslint:disable:no-unused-variable
// function asciiToInt(s: string): number {
//   let result: number = 0
//   for (let i = 0; i < s.length; i++) {
//     let char = s.charCodeAt(i)
//     result |= char
//     if (i < s.length - 1) {
//       result <<= 8
//     }
//   }
//   return result
// }
