
import { SceneStateDefinition } from './SceneStateDefinition'
import { Component, ComponentData, ComponentId, EntityId, StateContainer, StatefulActor } from './types'

export class SceneStateActor extends StatefulActor {
  constructor(private sceneState: SceneStateDefinition) {
    super()
  }

  addEntity(entityId: EntityId, components?: Component[]): void {
    this.sceneState.addEntity(entityId, components)
  }

  removeEntity(entityId: EntityId): void {
    this.sceneState.removeEntity(entityId)
  }

  setComponent(entityId: EntityId, componentId: ComponentId, data: ComponentData): void {
    this.sceneState.setComponent(entityId, componentId, data)
  }

  removeComponent(entityId: EntityId, componentId: ComponentId): void {
    this.sceneState.removeComponent(entityId, componentId)
  }

  sendStateTo(container: StateContainer) {
    this.sceneState.sendStateTo(container)
  }

  getState(): SceneStateDefinition {
    return this.sceneState
  }
}
