import { CatalystClient } from 'dcl-catalyst-client'
import { EntityType } from 'dcl-catalyst-commons'
import { SceneStateDefinition } from './SceneStateDefinition'
import { ILand } from 'shared/types'
import { SceneStateStorageController } from 'shared/apis/SceneStateStorageController/SceneStateStorageController'
import { deserializeSceneState, serializeSceneState } from './SceneStateDefinitionSerializer'

declare var window: any

export class BuilderStatefulActor {

  private lastTimeUpdated =  0
  private times = 0
  constructor(protected readonly land: ILand, protected readonly realmDomain: string, private readonly sceneStorage : SceneStateStorageController) 
  {
  }

  sceneStateChange(sceneState: SceneStateDefinition): void {
    if(this.lastTimeUpdated < Date.now().valueOf())
    {

    this.lastTimeUpdated = Date.now().valueOf()+2000
    this.times++
    console.warn("time saved" + this.times)
    this.sceneStorage.saveProjectManifest(serializeSceneState(sceneState))
    }
  }


  async getInititalSceneState(): Promise<SceneStateDefinition> {
    const sceneState = await this.getContentLandDefinition()
    return sceneState ? sceneState : new SceneStateDefinition()
  }

  async getContentLandDefinition(): Promise<SceneStateDefinition | undefined> {
    const builderProjectByCoordinates = await this.sceneStorage.getProjectManifestByCoordinates(this.land.sceneJsonData.scene.base)
    console.log("Projecto busdcado por coordinadas " + JSON.stringify(builderProjectByCoordinates))
    if(builderProjectByCoordinates)
    {
      console.log("Projecto por coordenadas encontrado")
      return deserializeSceneState(builderProjectByCoordinates)
    }

    const catalyst = this.getContentClient();
    const entity = await catalyst.fetchEntityById(EntityType.SCENE, this.land.sceneId)

    if (entity.metadata.source?.projectId) {
      console.log('Scene del builder encontrada con id' + entity.metadata?.source?.projectId)
      const builderProject = await this.sceneStorage.getProjectManifest(entity.metadata.source.projectId)
      console.log('Projecto del builder con id ' + entity.metadata?.source?.projectId + ' encontrado con los datos ' +  JSON.stringify(builderProject))
      if(builderProject) {
        return deserializeSceneState(builderProject)
      }
    }

    console.log("No encontrado ningun projecto asociado al builder, o builder in world, creando uno nuevo")
    const newProject = await this.sceneStorage.createProjectWithCoords(this.land)
    return newProject ? deserializeSceneState(newProject) : new SceneStateDefinition()
  }

 

  saveToLocalStorage(key: string, data: any) {
    if (!window.localStorage) {
      throw new Error('Storage not supported')
    }
    window.localStorage.setItem(key, JSON.stringify(data))
  }

  getFromLocalStorage(key: string) {
    if (!window.localStorage) {
      throw new Error('Storage not supported')
    }
    const data = window.localStorage.getItem(key)
    return (data && JSON.parse(data)) || null
  }

  private getContentClient(): CatalystClient {
    return new CatalystClient(this.realmDomain, 'builder in-world')
  }
}
