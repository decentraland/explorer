import { CLASS_ID } from 'decentraland-ecs/src'
import { SceneStateDefinition } from 'scene-system/stateful-scene/SceneStateDefinition'
import { Component } from 'scene-system/stateful-scene/types'
import { uuid } from 'decentraland-ecs/src/ecs/helpers'
import {
  BuilderAsset,
  BuilderComponent,
  BuilderEntity,
  BuilderManifest,
  BuilderScene,
  SerializedSceneState,
  UnityColor
} from './types'
import { BuilderServerAPIManager } from './BuilderServerAPIManager'

const CURRENT_SCHEMA_VERSION = 1

export type StorableSceneState = {
  schemaVersion: number
  entities: StorableEntity[]
}

type StorableEntity = {
  id: string
  components: StorableComponent[]
}

type StorableComponent = {
  type: string
  value: any
}

export async function toBuilderFromStateDefinitionFormat(
  scene: SceneStateDefinition,
  builderManifest: BuilderManifest,
  builderApiManager: BuilderServerAPIManager
): Promise<BuilderManifest> {
  let entities: Record<string, BuilderEntity> = {}
  let builderComponents: Record<string, BuilderComponent> = {}

  // Iterate every entity to get the components for builder
  for (const [entityId, components] of scene.getState().entries()) {
    let builderComponentsIds: string[] = []

    // Iterate the entity components to transform them to the builder format
    const mappedComponents = Array.from(components.entries()).map(([componentId, data]) => ({ componentId, data }))
    for (let component of mappedComponents) {
      // We generate a new uuid for the component since there is no uuid for components in the stateful scheme
      let newId = uuid()

      let componentType = toHumanReadableType(component.componentId)
      builderComponentsIds.push(newId)

      // This is a special case where we are assinging the builder url field for NFTs
      if (componentType === 'NFTShape') {
        component.data.url = component.data.src
      }

      // we add the component to the builder format
      let builderComponent: BuilderComponent = {
        id: newId,
        type: componentType,
        data: component.data
      }
      builderComponents[builderComponent.id] = builderComponent
    }

    // we add the entity to builder format
    let builderEntity: BuilderEntity = {
      id: entityId,
      components: builderComponentsIds,
      disableGizmos: false,
      name: entityId
    }
    entities[builderEntity.id] = builderEntity
  }

  // We create the scene and add it to the manifest
  const sceneState: BuilderScene = {
    id: builderManifest.scene.id,
    entities: entities,
    components: builderComponents,
    assets: builderManifest.scene.assets,
    metrics: builderManifest.scene.metrics,
    limits: builderManifest.scene.limits,
    ground: builderManifest.scene.ground
  }

  builderManifest.scene = sceneState

  // We get all the assetIds from the gltfShapes so we can fetch the corresponded asset
  let idArray: string[] = []
  Object.values(builderManifest.scene.components).forEach((component) => {
    if (component.type === 'GLTFShape') {
      let found = false
      Object.keys(builderManifest.scene.assets).forEach((assets) => {
        if (assets === component.data.assetId) {
          found = true
        }
      })
      if (!found) {
        idArray.push(component.data.assetId)
      }
    }
  })

  // We fetch all the assets that the scene contains since builder needs the assets
  const newAssets = await builderApiManager.getAssets(idArray)
  for (const [key, value] of Object.entries(newAssets)) {
    builderManifest.scene.assets[key] = value
  }

  // We remove unused assets
  let newRecords: Record<string, BuilderAsset> = {}
  for (const [key, value] of Object.entries(builderManifest.scene.assets)) {
    let found = false
    Object.values(builderManifest.scene.components).forEach((component) => {
      if (component.type === 'GLTFShape') {
        if (component.data.assetId === key) found = true
      }
    })

    if (found) {
      newRecords[key] = value
    }
  }

  builderManifest.scene.assets = newRecords

  // This is a special case. The builder needs the ground separated from the rest of the components so we search for it.
  // Unity handles this, so we will find only the same "ground" category. We can safely assume that we can search it and assign
  let groundComponentId: string
  Object.entries(builderManifest.scene.assets).forEach(([assetId, asset]) => {
    if (asset?.category === 'ground') {
      builderManifest.scene.ground.assetId = assetId
      Object.entries(builderManifest.scene.components).forEach(([componentId, component]) => {
        if (component.data.assetId === assetId) {
          builderManifest.scene.ground.componentId = componentId
          groundComponentId = componentId
        }
      })
    }
  })

  // We should disable the gizmos of the floor in the builder
  Object.values(builderManifest.scene.entities).forEach((entity) => {
    Object.values(entity.components).forEach((componentId) => {
      if (componentId === groundComponentId) {
        entity.disableGizmos = true
      }
    })
  })

  return builderManifest
}

export function fromBuildertoStateDefinitionFormat(scene: BuilderScene): SceneStateDefinition {
  const sceneState = new SceneStateDefinition()

  const componentMap = new Map(Object.entries(scene.components))

  for (let entity of Object.values(scene.entities)) {
    let components: Component[] = []
    for (let componentId of entity.components.values()) {
      if (componentMap.has(componentId)) {
        const builderComponent = componentMap.get(componentId)
        const componentData = builderComponent?.data

        // Builder set different the NFTs so we need to create a model that Unity is capable to understand,
        if (!componentData.hasOwnProperty('src') && builderComponent?.type === 'NFTShape') {
          let newAssetId = componentData.url.replaceAll('ethereum://', '')
          const index = newAssetId.indexOf('/')
          const partToRemove = newAssetId.slice(index)
          newAssetId = newAssetId.replaceAll(partToRemove, '')

          const color: UnityColor = {
            r: 0.6404918,
            g: 0.611472,
            b: 0.8584906,
            a: 1
          }
          componentData.src = componentData.url
          componentData.assetId = newAssetId
          componentData.color = color
          componentData.style = 0
        }
        let component: Component = {
          componentId: fromHumanReadableType(componentMap.get(componentId)!.type),
          data: componentData
        }
        components.push(component)
      }
    }

    sceneState.addEntity(entity.id, components)
  }
  return sceneState
}

export function fromSerializedStateToStorableFormat(state: SerializedSceneState): StorableSceneState {
  const entities = state.entities.map(({ id, components }) => ({
    id,
    components: components.map(({ type, value }) => ({ type: toHumanReadableType(type), value }))
  }))
  return {
    schemaVersion: CURRENT_SCHEMA_VERSION,
    entities
  }
}

export function fromStorableFormatToSerializedState(state: StorableSceneState): SerializedSceneState {
  const entities = state.entities.map(({ id, components }) => ({
    id,
    components: components.map(({ type, value }) => ({ type: fromHumanReadableType(type), value }))
  }))
  return { entities }
}

/**
 * We are converting from numeric ids to a more human readable format. It might make sense to change this in the future,
 * but until this feature is stable enough, it's better to store it in a way that it is easy to debug.
 */

const HUMAN_READABLE_TO_ID: Map<string, number> = new Map([
  ['Transform', CLASS_ID.TRANSFORM],
  ['GLTFShape', CLASS_ID.GLTF_SHAPE],
  ['NFTShape', CLASS_ID.NFT_SHAPE],
  ['Name', CLASS_ID.NAME],
  ['LockedOnEdit', CLASS_ID.LOCKED_ON_EDIT],
  ['VisibleOnEdit', CLASS_ID.VISIBLE_ON_EDIT]
])

function toHumanReadableType(type: number): string {
  const humanReadableType = Array.from(HUMAN_READABLE_TO_ID.entries())
    .filter(([, componentId]) => componentId === type)
    .map(([type]) => type)[0]
  if (!humanReadableType) {
    throw new Error(`Unknown type ${type}`)
  }
  return humanReadableType
}

function fromHumanReadableType(humanReadableType: string): number {
  const type = HUMAN_READABLE_TO_ID.get(humanReadableType)
  if (!type) {
    throw new Error(`Unknown human readable type ${humanReadableType}`)
  }
  return type
}
