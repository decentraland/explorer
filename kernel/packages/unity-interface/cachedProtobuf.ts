import {
  PB_AttachEntityComponent,
  PB_ComponentCreated,
  PB_ComponentDisposed,
  PB_ComponentRemoved,
  PB_ComponentUpdated,
  PB_CreateEntity,
  PB_OpenExternalUrl,
  PB_OpenNFTDialog,
  PB_Query,
  PB_Ray,
  PB_RayQuery,
  PB_RemoveEntity,
  PB_SetEntityParent,
  PB_UpdateEntityComponent,
  PB_Vector3
} from '../shared/proto/engineinterface_pb'

export const createEntity: PB_CreateEntity = new PB_CreateEntity()
export const removeEntity: PB_RemoveEntity = new PB_RemoveEntity()
export const updateEntityComponent: PB_UpdateEntityComponent = new PB_UpdateEntityComponent()
export const attachEntity: PB_AttachEntityComponent = new PB_AttachEntityComponent()
export const removeEntityComponent: PB_ComponentRemoved = new PB_ComponentRemoved()
export const setEntityParent: PB_SetEntityParent = new PB_SetEntityParent()
export const query: PB_Query = new PB_Query()
export const rayQuery: PB_RayQuery = new PB_RayQuery()
export const ray: PB_Ray = new PB_Ray()
export const origin: PB_Vector3 = new PB_Vector3()
export const direction: PB_Vector3 = new PB_Vector3()
export const componentCreated: PB_ComponentCreated = new PB_ComponentCreated()
export const componentDisposed: PB_ComponentDisposed = new PB_ComponentDisposed()
export const componentUpdated: PB_ComponentUpdated = new PB_ComponentUpdated()
export const openExternalUrl: PB_OpenExternalUrl = new PB_OpenExternalUrl()
export const openNFTDialog: PB_OpenNFTDialog = new PB_OpenNFTDialog()
