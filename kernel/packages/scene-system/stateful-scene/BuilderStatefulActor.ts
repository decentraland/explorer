﻿import { SceneStateDefinition } from './SceneStateDefinition'
import { ILand } from 'shared/types'
import { deserializeSceneState } from './SceneStateDefinitionSerializer'
import { ISceneStateStorageController } from 'shared/apis/SceneStateStorageController/ISceneStateStorageController'
import { CONTENT_PATH } from 'shared/apis/SceneStateStorageController/types'

export class BuilderStatefulActor {
  constructor(protected readonly land: ILand, private readonly sceneStorage: ISceneStateStorageController) {}

  async getInititalSceneState(): Promise<SceneStateDefinition> {
    const sceneState = await this.getContentLandDefinition()
    return sceneState ? sceneState : new SceneStateDefinition()
  }

  private async getContentLandDefinition(): Promise<SceneStateDefinition | undefined> {
    // Fetch project from builder api
    if (this.land.sceneJsonData.source?.projectId) {
      const builderProject = await this.sceneStorage.getProjectManifest(this.land.sceneJsonData.source?.projectId)
      if (builderProject) {
        return deserializeSceneState(builderProject)
      }
    }

    // Look for stateful definition
    if (this.land.mappingsResponse.contents.find((pair) => pair.file === CONTENT_PATH.DEFINITION_FILE)) {
      const land = this.land
      const definition = await this.sceneStorage.createProjectFromStateDefinition(
        land.sceneId,
        land.sceneJsonData.source?.projectId,
        land.sceneJsonData.scene.base,
        land.sceneJsonData.scene.parcels,
        land.sceneJsonData.display?.title,
        land.sceneJsonData.display?.description
      )
      if (definition) {
        return deserializeSceneState(definition)
      }
    }

    // Try with it coordinates if failed
    const builderProjectByCoordinates = await this.sceneStorage.getProjectManifestByCoordinates(
      this.land.sceneJsonData.scene.base
    )
    if (builderProjectByCoordinates) {
      return deserializeSceneState(builderProjectByCoordinates)
    }

    // If there is no builder project deployed in the land, we just create a new one
    await this.sceneStorage.createProjectWithCoords(this.land.sceneJsonData.scene.base)
    return new SceneStateDefinition()
  }
}
