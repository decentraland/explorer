import { SerializedSceneState, DeploymentResult } from './types'

export interface ISceneStateStorageController {
  publishSceneState(sceneId: string, sceneName: string, sceneDescription: string, sceneScreenshot: string, sceneState: SerializedSceneState): Promise<DeploymentResult>
  getStoredState(sceneId: string): Promise<SerializedSceneState | undefined>
  saveSceneState(serializedSceneState: SerializedSceneState): Promise<DeploymentResult>
  getProjectManifest(projectId: string): Promise<SerializedSceneState | undefined>
  getProjectManifestByCoordinates(land: string): Promise<SerializedSceneState | undefined>
  createProjectWithCoords(coordinates: string): Promise<boolean>
  createProjectFromStateDefinition(): Promise<SerializedSceneState | undefined>
  saveSceneInfo(sceneState: SerializedSceneState, sceneName: string, sceneDescription: string, sceneScreenshot: string): Promise<boolean>
}
