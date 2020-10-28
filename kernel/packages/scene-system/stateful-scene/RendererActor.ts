import { CLASS_ID } from "decentraland-ecs/src";
import { IEngineAPI } from "shared/apis/EngineAPI";
import { AttachEntityComponentPayload, ComponentCreatedPayload, ComponentRemovedPayload, ComponentUpdatedPayload, CreateEntityPayload, EntityAction, RemoveEntityPayload, UpdateEntityComponentPayload } from "shared/types";
import { Component, ComponentData, ComponentId, EntityId, StatefulActor } from "./types"
import { EventSubscriber } from "decentraland-rpc";
import { generatePBObjectJSON } from "scene-system/sdk/Utils";

export class RendererActor extends StatefulActor {

  private readonly eventSubscriber: EventSubscriber
  private components: number = 0;

  constructor(
    private readonly engine: IEngineAPI,
    private readonly sceneId: string) {
    super()
    this.eventSubscriber = new EventSubscriber(this.engine)
  }

  addEntity(entityId: EntityId, components?: Component[]): void {
    const batch: EntityAction[] = [{
      type: 'CreateEntity',
      payload: { id: entityId } as CreateEntityPayload
    }]
    if (components) {
      components.map(({ id, data }) => this.mapComponent(entityId, id, data))
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

  setComponent(entityId: EntityId, componentId: ComponentId, data: ComponentData): void {
    const updates = this.mapComponent(entityId, componentId, data)
    this.engine.sendBatch(updates)
  }

  removeComponent(entityId: EntityId, componentId: ComponentId): void {
    const { name } = this.componentTypeToLegacyData(componentId)
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

  onAddEntity(listener: (entityId: EntityId, components?: Component[]) => void): void {
    this.eventSubscriber.on('stateEvent', ({ type, payload }) => {
      if (type === 'AddEntity') {
        listener(payload.entityId, payload.components)
      }
    })
  }

  onRemoveEntity(listener: (entityId: EntityId) => void): void {
    this.eventSubscriber.on('stateEvent', ({ type, payload }) => {
      if (type === 'RemoveEntity') {
        listener(payload.entityId)
      }
    })
  }

  onSetComponent(listener: (entityId: EntityId, componentId: ComponentId, data: ComponentData) => void): void {
    this.eventSubscriber.on('stateEvent', ({ type, payload }) => {
      if (type === 'SetComponent') {
        listener(payload.entityId, payload.componentId, payload.componentData)
      }
    })
  }

  onRemoveComponent(listener: (entityId: EntityId, componentId: ComponentId) => void): void {
    this.eventSubscriber.on('stateEvent', ({ type, payload }) => {
      if (type === 'RemoveComponent') {
        listener(payload.entityId, payload.componentId)
      }
    })
  }

  private mapComponent(entityId: EntityId, componentId: ComponentId, data: ComponentData): EntityAction[] {
    const { disposability, defaultValue } = this.componentTypeToLegacyData(componentId)
    const finalData = Object.assign(defaultValue ?? {}, data)
    if (disposability === ComponentDisposability.DISPOSABLE) {
      return this.buildDisposableComponentActions(entityId, componentId, finalData)
    } else {
      return [{
        type: 'UpdateEntityComponent',
        tag: this.sceneId + '_' + entityId + '_' + componentId,
        payload: {
          entityId,
          classId: componentId,
          json: generatePBObjectJSON(componentId, finalData)
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

  // TODO: We need to figure out a better way to handle defaults, so we can try to re-use the logic that already exists
  private componentTypeToLegacyData(componentId: ComponentId): { name: string, disposability: ComponentDisposability, defaultValue?: ComponentData } {
    switch (componentId) {
      case CLASS_ID.TRANSFORM:
        return {
          name: 'transform',
          disposability: ComponentDisposability.NON_DISPOSABLE,
          defaultValue: { position: { x: 0, y: 0, z: 0 }, rotation: { x: 0.0, y: 0.0, z: 0.0, w: 1.0 }, scale: { x: 1, y: 1, z: 1 } }
        }
      case CLASS_ID.GLTF_SHAPE:
        return { name: 'shape', disposability: ComponentDisposability.DISPOSABLE }
    }
    throw new Error('Not implemented yet')
  }

}

enum ComponentDisposability {
  DISPOSABLE,
  NON_DISPOSABLE
}