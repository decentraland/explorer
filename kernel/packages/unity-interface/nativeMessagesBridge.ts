import {
  CreateEntityPayload,
  RemoveEntityPayload,
  EntityAction,
  QueryPayload,
  LoadableParcelScene,
  SetEntityParentPayload,
  OpenNFTDialogPayload,
  ComponentUpdatedPayload,
  ComponentRemovedPayload,
  ComponentDisposedPayload,
  ComponentCreatedPayload,
  AttachEntityComponentPayload,
  UpdateEntityComponentPayload,
  EntityActionType
} from 'shared/types'
import { PB_OpenExternalUrl } from 'shared/proto/engineinterface_pb'

export class NativeMessagesBridge {
  private __createEntity!: () => void
  private __removeEntity!: () => void
  private __sceneReady!: () => void
  private __setSceneId!: (sceneId: string) => void
  private __setEntityId!: (entityId: string) => void
  private __setEntityParent!: (parentId: string) => void

  private __entityComponentCreateOrUpdate!: (classId: number, json: string) => void
  private __entityComponentDestroy!: (name: string) => void

  private __sharedComponentCreate!: (classId: number, id: string, name: string) => void
  private __sharedComponentAttach!: (id: string, name: string) => void
  private __sharedComponentUpdate!: (id: string, json: string) => void
  private __sharedComponentDispose!: (id: string) => void

  private __openNftDialog!: (contactAddress: string, comment: string) => void
  private __openExternalUrl!: (url: string) => void
  private __query!: (payload: QueryPayload) => void

  private __loadParcelScene!: (scene: LoadableParcelScene) => void
  private __updateParcelScene!: (scene: LoadableParcelScene) => void
  private __unloadParcelScene!: (sceneId: string) => void

  private currentSceneId: string = ''
  private currentEntityId: string = ''

  //private currentTag: number = 0

  public initNativeMessages(gameInstance: any) {
    let unityModule: any = gameInstance.Module

    if (!unityModule) {
      console.error('Unity module not found! Are you in WSS mode?')
      return
    }

    // this.__loadParcelScene = unityModule.cwrap('call_LoadParcelScene', null, ['number'])
    // this.__updateParcelScene = unityModule.cwrap('call_UpdateParcelScene', null, ['number'])
    // this.__unloadParcelScene = unityModule.cwrap('call_UnloadParcelScene', null, ['string'])

    this.__setEntityId = unityModule.cwrap('call_SetEntityId', null, ['string'])
    this.__setSceneId = unityModule.cwrap('call_SetSceneId', null, ['string'])
    this.__setEntityParent = unityModule.cwrap('call_SetEntityParent', null, ['string'])
    
    this.__entityComponentCreateOrUpdate = unityModule.cwrap('call_EntityComponentCreateOrUpdate', null, [
      'number',
      'string'
    ])
    this.__entityComponentDestroy = unityModule.cwrap('call_EntityComponentRemove', null, ['string'])

    this.__sharedComponentCreate = unityModule.cwrap('call_SharedComponentCreate', null, ['number', 'string'])
    this.__sharedComponentAttach = unityModule.cwrap('call_SharedComponentAttach', null, ['string', 'string'])
    this.__sharedComponentUpdate = unityModule.cwrap('call_SharedComponentUpdate', null, ['string', 'string'])
    this.__sharedComponentDispose = unityModule.cwrap('call_SharedComponentDispose', null, ['string'])
    
    this.__openNftDialog = unityModule.cwrap('call_OpenNftDialog', null, ['string', 'string'])
    this.__openExternalUrl = unityModule.cwrap('call_OpenExternalUrl', null, ['string'])
    this.__query = unityModule.cwrap('call_Query', null, ['number'])

    this.__createEntity = unityModule.cwrap('call_CreateEntity', null, [])
    this.__removeEntity = unityModule.cwrap('call_RemoveEntity', null, [])
    this.__sceneReady = unityModule.cwrap('call_SceneReady', null, [])
    console.log('Init native messages...')
  }

  public optimizeSendMessage() {
    //no-op
  }

  // | 'UpdateEntityComponent'
  // | 'AttachEntityComponent'
  // | 'ComponentCreated'
  // | 'ComponentDisposed'
  // | 'ComponentRemoved'
  // | 'ComponentUpdated'
  public isMethodSupported(method: EntityActionType): boolean {
    return method !== 'Query'
    // return (
    //   method === 'CreateEntity' ||
    //   method === 'RemoveEntity' ||
    //   method === 'SetEntityParent' ||
    //   method === 'InitMessagesFinished' ||
    //   method === 'UpdateEntityComponent' ||
    //   method === 'AttachEntityComponent' ||
    // )
  }

  setSceneId(sceneId: string) {
    if (sceneId !== this.currentSceneId) {
      this.__setSceneId(sceneId)
    }
    this.currentSceneId = sceneId
  }

  setEntityId(entityId: string) {
    if (entityId !== this.currentEntityId) {
      this.__setEntityId(entityId)
    }
    this.currentEntityId = entityId
  }

  setTag(tag: string) {
    //this.currentTag = tag
  }

  createEntity(payload: CreateEntityPayload) {
    this.setEntityId(payload.id)
    this.__createEntity()
  }

  removeEntity(payload: RemoveEntityPayload) {
    this.setEntityId(payload.id)
    this.__removeEntity()
  }

  sceneReady() {
    this.__sceneReady()
  }

  setEntityParent(payload: SetEntityParentPayload) {
    this.setEntityId(payload.entityId)
    this.__setEntityParent(payload.parentId)
  }

  openNftDialog(payload: OpenNFTDialogPayload) {
    this.__openNftDialog(payload.assetContractAddress, payload.comment ?? '')
  }

  openExternalUrl(payload: PB_OpenExternalUrl) {
    this.__openExternalUrl(payload.getUrl())
  }

  query(payload: QueryPayload) {
    this.__query(payload)
  }

  sharedComponentUpdate(payload: ComponentUpdatedPayload) {
    this.__sharedComponentUpdate(payload.id, payload.json)
  }

  entityComponentRemove(payload: ComponentRemovedPayload) {
    this.setEntityId(payload.entityId)
    this.__entityComponentDestroy(payload.name)
  }

  sharedComponentDispose(payload: ComponentDisposedPayload) {
    this.__sharedComponentDispose(payload.id)
  }

  sharedComponentCreate(payload: ComponentCreatedPayload) {
    this.__sharedComponentCreate(payload.classId, payload.id, payload.name)
  }

  sharedComponentAttach(payload: AttachEntityComponentPayload) {
    this.setEntityId(payload.entityId)
    this.__sharedComponentAttach(payload.id, payload.name)
  }

  entityComponentCreateOrUpdate(payload: UpdateEntityComponentPayload) {
    this.setEntityId(payload.entityId)
    this.__entityComponentCreateOrUpdate(payload.classId, payload.json)
  }

  public loadParcelScene(loadableParcelScene: LoadableParcelScene) {
    this.__loadParcelScene(loadableParcelScene)
  }

  public updateParcelScene(loadableParcelScene: LoadableParcelScene) {
    this.__updateParcelScene(loadableParcelScene)
  }

  public unloadParcelScene(sceneId: string) {
    this.__unloadParcelScene(sceneId)
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
        this.sceneReady()
        break
      case 'SetEntityParent':
        this.setEntityParent(action.payload)
        break
      case 'UpdateEntityComponent':
        this.entityComponentCreateOrUpdate(action.payload)
        break
      case 'ComponentRemoved':
        this.entityComponentRemove(action.payload)
        break
      case 'AttachEntityComponent':
        this.sharedComponentAttach(action.payload)
        break
      case 'ComponentCreated':
        this.sharedComponentCreate(action.payload)
        break
      case 'ComponentDisposed':
        this.sharedComponentDispose(action.payload)
        break
      case 'ComponentUpdated':
        this.sharedComponentUpdate(action.payload)
        break
      case 'Query':
        this.query(action.payload)
        break
      case 'OpenExternalUrl':
        this.openExternalUrl(action.payload)
        break
      case 'OpenNFTDialog':
        this.openNftDialog(action.payload)
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
