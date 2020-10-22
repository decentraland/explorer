import { CLASS_ID, Transform } from "decentraland-ecs/src";
import { IEngineAPI } from "shared/apis/EngineAPI";
import { PB_Quaternion, PB_Transform, PB_Vector3 } from "../../shared/proto/engineinterface_pb";
import { AttachEntityComponentPayload, ComponentCreatedPayload, ComponentRemovedPayload, ComponentUpdatedPayload, CreateEntityPayload, EntityAction, RemoveEntityPayload, UpdateEntityComponentPayload } from "shared/types";
import { Component, ComponentData, ComponentType, EntityId, StateActor } from "./StateActor"

const pbTransform: PB_Transform = new PB_Transform()
const pbPosition: PB_Vector3 = new PB_Vector3()
const pbRotation: PB_Quaternion = new PB_Quaternion()
const pbScale: PB_Vector3 = new PB_Vector3()

export class UnityActor implements StateActor {

  private components: number = 0;

  constructor(
    private readonly engine: IEngineAPI,
    private readonly sceneId: string) { }

  addEntity(entityId: EntityId, components?: Component[]): void {
    const batch: EntityAction[] = [{
      type: 'CreateEntity',
      payload: { id: entityId } as CreateEntityPayload
    }]
    if (components) {
      components.map(({ type, data }) => this.mapComponent(entityId, type, data))
        .forEach(actions => batch.push(...actions))
    }
    this.engine.sendBatch(batch)
  }

  removeEntity(entityId: EntityId): void {
    this.engine.sendBatch([{
      type: 'RemoveEntity',
      payload: { id: entityId } as RemoveEntityPayload
    }])
  }

  setComponent(entityId: EntityId, componentType: ComponentType, data: ComponentData): void {
    const updates = this.mapComponent(entityId, componentType, data)
    this.engine.sendBatch(updates)
  }

  removeComponent(entityId: EntityId, componentType: ComponentType): void {
    const { name } = this.componentTypeToLegacyData(componentType)
    this.engine.sendBatch([{
      type: 'ComponentRemoved',
      tag: entityId,
      payload: {
        entityId,
        name
      } as ComponentRemovedPayload
    }])
  }

  sendInitFinished() {
    this.engine.sendBatch([{
      type: 'InitMessagesFinished',
      tag: 'scene',
      payload: '{}'
    }])
  }

  // TODO
  onAddEntity(listener: (entityId: EntityId, components?: Component[]) => void): void { }
  onRemoveEntity(listener: (entityId: EntityId) => void): void { }
  onSetComponent(listener: (entityId: EntityId, componentType: ComponentType, data: ComponentData) => void): void { }
  onRemoveComponent(listener: (entityId: EntityId, componentType: ComponentType) => void): void { }


  private mapComponent(entityId: EntityId, type: ComponentType, data: ComponentData): EntityAction[] {
    const { classId, disposability } = this.componentTypeToLegacyData(type)
    if (disposability === ComponentDisposability.DISPOSABLE) {
      return this.buildDisposableComponentActions(entityId, classId, data)
    } else {
      return [{
        type: 'UpdateEntityComponent',
        tag: this.sceneId + '_' + entityId + '_' + classId,
        payload: {
          entityId,
          classId,
          json: this.generatePBObject(classId, data)
        } as UpdateEntityComponentPayload
      }]
    }
  }

  private buildDisposableComponentActions(entityId: EntityId, classId: number, data: ComponentData): EntityAction[] {
    const id = `C${this.components++}`
    return [
      {
        type: 'ComponentCreated',
        tag: id,
        payload: {
          id,
          classId,
        } as ComponentCreatedPayload
      },
      {
        type: 'ComponentUpdated',
        tag: id,
        payload: {
          id,
          json: JSON.stringify(data)
        } as ComponentUpdatedPayload
      },
      {
        type: 'AttachEntityComponent',
        tag: entityId,
        payload: {
          entityId,
          id
        } as AttachEntityComponentPayload
      }
    ]
  }

  private generatePBObject(classId: CLASS_ID, data: ComponentData): string {
    if (classId === CLASS_ID.TRANSFORM) {
      const transform: Transform = data

      pbPosition.setX(Math.fround(transform.position?.x ?? 0))
      pbPosition.setY(Math.fround(transform.position?.y ?? 0))
      pbPosition.setZ(Math.fround(transform.position?.z ?? 0))

      pbRotation.setX(transform.rotation?.x ?? 0)
      pbRotation.setY(transform.rotation?.y ?? 0)
      pbRotation.setZ(transform.rotation?.z ?? 0)
      pbRotation.setW(transform.rotation?.w ?? 1)

      pbScale.setX(Math.fround(transform.scale?.x ?? 1))
      pbScale.setY(Math.fround(transform.scale?.y ?? 1))
      pbScale.setZ(Math.fround(transform.scale?.z ?? 1))

      pbTransform.setPosition(pbPosition)
      pbTransform.setRotation(pbRotation)
      pbTransform.setScale(pbScale)

      let arrayBuffer: Uint8Array = pbTransform.serializeBinary()
      return btoa(String.fromCharCode(...arrayBuffer))
    }
    return JSON.stringify(data)
  }

  private componentTypeToLegacyData(type: ComponentType): { name: string, classId: number, disposability: ComponentDisposability } {
    switch (type) {
      case 'Transform':
        return { name: 'transform', classId: CLASS_ID.TRANSFORM, disposability: ComponentDisposability.NON_DISPOSABLE }
      case 'GLTFShape':
        return { name: 'shape', classId: CLASS_ID.GLTF_SHAPE, disposability: ComponentDisposability.DISPOSABLE }
    }
    throw new Error('Not implemented yet')
  }

}

enum ComponentDisposability {
  DISPOSABLE,
  NON_DISPOSABLE
}