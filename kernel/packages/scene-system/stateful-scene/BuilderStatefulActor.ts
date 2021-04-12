import { CatalystClient } from 'dcl-catalyst-client'
import { EntityType } from 'dcl-catalyst-commons'
import { SceneStateDefinition } from './SceneStateDefinition'
import { ILand } from 'shared/types'
import { SceneStateStorageController } from 'shared/apis/SceneStateStorageController/SceneStateStorageController'
import { deserializeSceneState } from './SceneStateDefinitionSerializer'

declare var window: any

export class BuilderStatefulActor {
  constructor(
    protected readonly land: ILand,
    protected readonly realmDomain: string,
    private readonly sceneStorage: SceneStateStorageController
  ) {}

  async getInititalSceneState(): Promise<SceneStateDefinition> {
    const sceneState = await this.getContentLandDefinition()
    return sceneState ? sceneState : new SceneStateDefinition()
  }

  async getContentLandDefinition(): Promise<SceneStateDefinition | undefined> {

    //First we search the definition in the builder server filtering by land coordinates
    const builderProjectByCoordinates = await this.sceneStorage.getProjectManifestByCoordinates(
      this.land.sceneJsonData.scene.base
    )
    if (builderProjectByCoordinates) {
      return deserializeSceneState(builderProjectByCoordinates)
    }

    //if there is no project associated to the land, we search the last builder project deployed in the land
    const catalyst = this.getContentClient()
    const entity = await catalyst.fetchEntityById(EntityType.SCENE, this.land.sceneId)

    if (entity.metadata.source?.projectId) {
      const builderProject = await this.sceneStorage.getProjectManifest(entity.metadata.source.projectId)
      if (builderProject) {
        return deserializeSceneState(builderProject)
      }
    }

    //If there is no builder project deployed in the land, we just create a new one
    const newProject = await this.sceneStorage.createProjectWithCoords(this.land)
    return newProject ? deserializeSceneState(newProject) : new SceneStateDefinition()
  }

  private getContentClient(): CatalystClient {
    return new CatalystClient(this.realmDomain, 'builder-in-world')
  }
}
