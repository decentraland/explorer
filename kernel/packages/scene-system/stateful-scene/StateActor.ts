
export type EntityId = string
export type ComponentType = string
export type ComponentData = any
export type Component = {
  type: ComponentType
  data: ComponentData
}

export interface StateActor {

  addEntity(entityId: EntityId, components?: Component[]): void
  removeEntity(entityId: EntityId): void
  setComponent(entityId: EntityId, componentType: ComponentType, data: ComponentData): void
  removeComponent(entityId: EntityId, componentType: ComponentType): void

  onAddEntity(listener: (entityId: EntityId, components?: Component[]) => void): void
  onRemoveEntity(listener: (entityId: EntityId) => void): void
  onSetComponent(listener: (entityId: EntityId, componentType: ComponentType, data: ComponentData) => void): void
  onRemoveComponent(listener: (entityId: EntityId, componentType: ComponentType) => void): void

}

