import { Component } from 'scene-system/stateful-scene/types'
import { CLASS_ID, Matrix, Quaternion, Vector3 } from 'decentraland-ecs'
import { SceneSource, SceneSourcePlacement } from 'shared/types'
import { BuilderComponent } from './types'
import { toHumanReadableType } from './utils'
import { parcelLimits } from 'config'

/**
 * Utility class to translate the transformation that the builder dapp applies to the scene, while deploying,
 * to the stateful scene definition
 */

export class SceneTransformTranslator {
  private builderToStateDefinitionMatrix!: Matrix
  private stateDefinitionToBuilderMatrix!: Matrix

  public constructor(sceneSource?: SceneSource | undefined) {
    this.setBuilderSceneTransformation(sceneSource)
  }

  public setBuilderSceneTransformation(sceneSource: SceneSource | undefined) {
    const rotation: SceneSourcePlacement['rotation'] | undefined = sceneSource?.rotation
    const layout: SceneSourcePlacement['layout'] | undefined = sceneSource?.layout
    const parcelSize = parcelLimits.parcelSize

    if (!rotation || !layout) {
      this.builderToStateDefinitionMatrix = Matrix.Identity()
      this.stateDefinitionToBuilderMatrix = Matrix.Identity()
      return
    }

    const sceneRotation: Quaternion = Quaternion.Identity
    const sceneTranslation: Vector3 = Vector3.Zero()

    switch (rotation) {
      case 'north':
        sceneRotation.setEuler(0, -90, 0)
        sceneTranslation.set(layout.cols * parcelSize, 0, 0)
        break
      case 'east':
        break
      case 'south':
        sceneRotation.setEuler(0, 90, 0)
        sceneTranslation.set(0, 0, layout.rows * parcelSize)
        break
      case 'west':
        sceneRotation.setEuler(0, 180, 0)
        sceneTranslation.set(layout.rows * parcelSize, 0, layout.cols * parcelSize)
        break
    }

    this.builderToStateDefinitionMatrix = Matrix.Compose(Vector3.One(), sceneRotation, sceneTranslation)
    this.stateDefinitionToBuilderMatrix = Matrix.Invert(this.builderToStateDefinitionMatrix)
  }

  public transformBuilderComponent(builderComponent: Readonly<BuilderComponent>): BuilderComponent {
    const componentData = { ...builderComponent.data }

    if (builderComponent.type === toHumanReadableType(CLASS_ID.TRANSFORM)) {
      componentData.position = Vector3.TransformCoordinates(componentData.position, this.stateDefinitionToBuilderMatrix)
      let matrixRotation = Quaternion.Identity
      this.stateDefinitionToBuilderMatrix.decompose(undefined, matrixRotation, undefined)
      const componentRotation = new Quaternion(
        componentData.rotation.x,
        componentData.rotation.y,
        componentData.rotation.z,
        componentData.rotation.w
      )
      componentData.rotation = matrixRotation.multiply(componentRotation)
    }

    return { ...builderComponent, data: componentData }
  }

  public transformStateDefinitionComponent(statedDefinitionComponent: Readonly<Component>): Component {
    const transformMatrix = this.builderToStateDefinitionMatrix
    const componentData = { ...statedDefinitionComponent.data }

    if (statedDefinitionComponent.componentId === CLASS_ID.TRANSFORM) {
      componentData.position = Vector3.TransformCoordinates(componentData.position, transformMatrix)
      let matrixRotation = Quaternion.Identity
      transformMatrix.decompose(undefined, matrixRotation, undefined)
      const componentRotation = new Quaternion(
        componentData.rotation.x,
        componentData.rotation.y,
        componentData.rotation.z,
        componentData.rotation.w
      )
      componentData.rotation = matrixRotation.multiply(componentRotation)
    }

    return { ...statedDefinitionComponent, data: componentData }
  }
}
