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
         private __updateEntityComponent!: (classId: number, json: string) => void
         private __attachEntityComponent!: (name: string, id: string) => void
         private __removeComponent!: (name: string) => void
         private __openNftDialog!: (contactAddress: string, comment: string) => void
         private __openExternalUrl!: (url: string) => void
         private __componentUpdated!: (id: string, json: string) => void
         private __componentDisposed!: (id: string) => void
         private __componentCreated!: (id: string, classId: number, name: string) => void
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

           this.__loadParcelScene = unityModule.cwrap('call_LoadParcelScene', null, ['number'])
           this.__updateParcelScene = unityModule.cwrap('call_UpdateParcelScene', null, ['number'])
           this.__unloadParcelScene = unityModule.cwrap('call_UnloadParcelScene', null, ['string'])

           this.__setEntityId = unityModule.cwrap('call_SetEntityId', null, ['string'])
           this.__setSceneId = unityModule.cwrap('call_SetSceneId', null, ['string'])
           this.__setEntityParent = unityModule.cwrap('call_SetEntityParent', null, ['string'])
           this.__updateEntityComponent = unityModule.cwrap('call_UpdateEntityComponent', null, ['string', 'string'])
           this.__attachEntityComponent = unityModule.cwrap('call_AttachEntityComponent', null, ['string', 'string'])
           this.__removeComponent = unityModule.cwrap('call_RemoveComponent', null, ['string'])
           this.__openNftDialog = unityModule.cwrap('call_OpenNftDialog', null, ['string', 'string'])
           this.__openExternalUrl = unityModule.cwrap('call_OpenExternalUrl', null, ['string'])
           this.__componentUpdated = unityModule.cwrap('call_ComponentUpdated', null, ['string', 'string'])
           this.__componentDisposed = unityModule.cwrap('call_ComponentDisposed', null, ['string'])
           this.__componentCreated = unityModule.cwrap('call_ComponentCreated', null, ['string', 'string'])
           this.__query = unityModule.cwrap('call_Query', null, ['number'])

           this.__createEntity = unityModule.cwrap('call_CreateEntity', null, [])
           this.__removeEntity = unityModule.cwrap('call_RemoveEntity', null, [])
           this.__sceneReady = unityModule.cwrap('call_SceneReady', null, [])
           console.log('Init native messages...')
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

         componentUpdated(payload: ComponentUpdatedPayload) {
           this.__componentUpdated(payload.id, payload.json)
         }

         componentRemoved(payload: ComponentRemovedPayload) {
           this.__removeComponent(payload.name)
         }

         componentDisposed(payload: ComponentDisposedPayload) {
           this.__componentDisposed(payload.id)
         }

         componentCreated(payload: ComponentCreatedPayload) {
           this.__componentCreated(payload.id, payload.classId, payload.name)
         }

         attachEntityComponent(payload: AttachEntityComponentPayload) {
           this.__attachEntityComponent(payload.id, payload.name)
         }

         updateEntityComponent(payload: UpdateEntityComponentPayload) {
           this.__updateEntityComponent(payload.classId, payload.json)
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
               this.updateEntityComponent(action.payload)
               break
             case 'AttachEntityComponent':
               this.attachEntityComponent(action.payload)
               break
             case 'ComponentCreated':
               this.componentCreated(action.payload)
               break
             case 'ComponentDisposed':
               this.componentDisposed(action.payload)
               break
             case 'ComponentRemoved':
               this.componentRemoved(action.payload)
               break
             case 'ComponentUpdated':
               this.componentUpdated(action.payload)
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
