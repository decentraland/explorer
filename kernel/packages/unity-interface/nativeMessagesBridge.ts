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
import { QueryType } from 'decentraland-ecs/src'

export class NativeMessagesBridge {

  private __createEntity!: () => void
  private __removeEntity!: () => void
  private __sceneReady!: () => void

  private __setTag!: (tag: string) => void
  private __setSceneId!: (sceneId: string) => void
  private __setEntityId!: (entityId: string) => void

  private __setEntityParent!: (parentId: string) => void

  private __entityComponentCreateOrUpdate!: (classId: number, json: string) => void
  private __entityComponentDestroy!: (name: string) => void

  private __sharedComponentCreate!: (classId: number, id: string) => void
  private __sharedComponentAttach!: (id: string) => void
  private __sharedComponentUpdate!: (id: string, json: string) => void
  private __sharedComponentDispose!: (id: string) => void

  private __openNftDialog!: (contactAddress: string, comment: string) => void
  private __openExternalUrl!: (url: string) => void
  private __query!: (payload: number) => void

  private __loadParcelScene!: (scene: LoadableParcelScene) => void
  private __updateParcelScene!: (scene: LoadableParcelScene) => void
  private __unloadParcelScene!: (sceneId: string) => void

  private currentSceneId: string = ''
  private currentTag: string = ''
  private currentEntityId: string = ''

  private unityModule: any

  private queryMemBlockPtr: number = 0

  queryTypeToId( type:QueryType ) : number { 
    switch (type) {
      case 'HitFirst':
        return 1
      case 'HitAll':
        return 2
      case 'HitFirstAvatar':
        return 3
      case 'HitAllAvatars':
        return 4
      default:
        return 0
    }
  }

  public initNativeMessages(gameInstance: any) {
    this.unityModule = gameInstance.Module

    if (!this.unityModule) {
      console.error('Unity module not found! Are you in WSS mode?')
      return
    }

    const QUERY_MEM_SIZE = 40
    this.queryMemBlockPtr = this.unityModule._malloc(QUERY_MEM_SIZE)

    this.__setEntityId = this.unityModule.cwrap('call_SetEntityId', null, ['string'])
    this.__setSceneId = this.unityModule.cwrap('call_SetSceneId', null, ['string'])
    this.__setTag = this.unityModule.cwrap('call_SetTag', null, ['string'])

    this.__setEntityParent = this.unityModule.cwrap('call_SetEntityParent', null, ['string'])

    this.__entityComponentCreateOrUpdate = this.unityModule.cwrap('call_EntityComponentCreateOrUpdate', null, [
      'number',
      'string'
    ])

    this.__entityComponentDestroy = this.unityModule.cwrap('call_EntityComponentDestroy', null, ['string'])

    this.__sharedComponentCreate = this.unityModule.cwrap('call_SharedComponentCreate', null, ['number', 'string'])
    this.__sharedComponentAttach = this.unityModule.cwrap('call_SharedComponentAttach', null, ['string', 'string'])
    this.__sharedComponentUpdate = this.unityModule.cwrap('call_SharedComponentUpdate', null, ['string', 'string'])
    this.__sharedComponentDispose = this.unityModule.cwrap('call_SharedComponentDispose', null, ['string'])

    this.__openNftDialog = this.unityModule.cwrap('call_OpenNftDialog', null, ['string', 'string'])
    this.__openExternalUrl = this.unityModule.cwrap('call_OpenExternalUrl', null, ['string'])
    this.__query = this.unityModule.cwrap('call_Query', null, ['number'])

    this.__createEntity = this.unityModule.cwrap('call_CreateEntity', null, [])
    this.__removeEntity = this.unityModule.cwrap('call_RemoveEntity', null, [])
    this.__sceneReady = this.unityModule.cwrap('call_SceneReady', null, [])
  }

  public optimizeSendMessage() {
    //no-op
  }

  public isMethodSupported(method: EntityActionType): boolean {
    return true
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
    if (tag !== this.currentTag) {
      this.__setTag(tag)
    }
    this.currentTag = tag
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

  query(queryPayload: QueryPayload) {
    let alignedPtr = this.queryMemBlockPtr >> 2

    let queryType = this.queryTypeToId(queryPayload.queryId as QueryType)

    this.unityModule.HEAP32[alignedPtr++] = queryType
    this.unityModule.HEAP32[alignedPtr++] = parseInt(queryPayload.payload.queryId, 10)
    this.unityModule.HEAP32[alignedPtr++] = queryType
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.origin.x
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.origin.y
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.origin.z
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.direction.x
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.direction.y
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.direction.z
    this.unityModule.HEAPF32[alignedPtr++] = queryPayload.payload.ray.distance

    this.__query(this.queryMemBlockPtr)  
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
    this.__sharedComponentCreate(payload.classId, payload.id)
  }

  sharedComponentAttach(payload: AttachEntityComponentPayload) {
    this.setEntityId(payload.entityId)
    this.__sharedComponentAttach(payload.id)
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

    if (action.tag !== undefined) { 
      this.setTag(action.tag)
    }

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
