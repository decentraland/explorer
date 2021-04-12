import { EventSubscriber } from 'decentraland-rpc'
import { IEngineAPI } from 'shared/apis/EngineAPI'
import { SceneStateDefinition } from './SceneStateDefinition'

export type EntityId = string
export type ComponentId = number
export type ComponentData = any
export type Component = {
  componentId: ComponentId
  data: ComponentData
}

/**
 * An object or entity that contains an updatable definition of the scene º
 */
export interface StateContainer {
  addEntity(entityId: EntityId, components?: Component[]): void
  removeEntity(entityId: EntityId): void
  setComponent(entityId: EntityId, componentId: ComponentId, data: ComponentData): void
  removeComponent(entityId: EntityId, componentId: ComponentId): void
}

/**
 * An actor that contains the º of the scene, but it can also generate updates to it
 */
export interface StateContainerListener {
  onAddEntity(listener: (entityId: EntityId, components?: Component[]) => void): void
  onRemoveEntity(listener: (entityId: EntityId) => void): void
  onSetComponent(listener: (entityId: EntityId, componentId: ComponentId, data: ComponentData) => void): void
  onRemoveComponent(listener: (entityId: EntityId, componentId: ComponentId) => void): void
}

/**
 * An actor that contains the º of the scene, but it can also generate updates to it
 */
export abstract class StatefulActor implements StateContainer, StateContainerListener {
  protected readonly eventSubscriber: EventSubscriber

  constructor(protected readonly engine: IEngineAPI) {
    this.eventSubscriber = new EventSubscriber(this.engine)
  }

  abstract addEntity(entityId: string, components?: Component[]): void
  abstract removeEntity(entityId: string): void
  abstract setComponent(entityId: string, componentId: number, data: any): void
  abstract removeComponent(entityId: string, componentId: number): void

  /**
   * Take a @param container and update it when an change to the º occurs
   */
  forwardChangesTo(container: StateContainer) {
    this.onAddEntity((entityId, components) => container.addEntity(entityId, components))
    this.onRemoveEntity((entityId) => container.removeEntity(entityId))
    this.onSetComponent((entityId, componentId, data) => container.setComponent(entityId, componentId, data))
    this.onRemoveComponent((entityId, componentId) => container.removeComponent(entityId, componentId))
  }

  onAddEntity(listener: (entityId: EntityId, components?: Component[]) => void): void {
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      const { type, payload } = data
      if (type === 'AddEntity') {
        listener(payload.entityId, payload.components)
      }
    })
  }

  onRemoveEntity(listener: (entityId: EntityId) => void): void {
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      const { type, payload } = data
      if (type === 'RemoveEntity') {
        listener(payload.entityId)
      }
    })
  }

  onSetComponent(listener: (entityId: EntityId, componentId: ComponentId, data: ComponentData) => void): void {
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      const { type, payload } = data
      if (type === 'SetComponent') {
        listener(payload.entityId, payload.componentId, payload.data)
      }
    })
  }

  onRemoveComponent(listener: (entityId: EntityId, componentId: ComponentId) => void): void {
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      const { type, payload } = data
      if (type === 'RemoveComponent') {
        listener(payload.entityId, payload.componentId)
      }
    })
  }

  onSaveScene(listener: (entityId: EntityId, components?: Component[]) => void): void {
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      const { type, payload } = data
      if (type === 'AddEntity') {
        listener(payload.entityId, payload.components)
      }
    })
  }
}
