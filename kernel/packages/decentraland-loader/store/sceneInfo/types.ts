export type StringCoordinate = string
export type SceneId = string // Qm...

export type Mapping = {
  file: string
  hash: string
}
export const CLOSENESS_RADIUS_FOR_GARBAGE_COLLECTING = 15
export const INVALID_STATE = 'Invalid State'
export type InvalidState = typeof INVALID_STATE
export type SceneMetadata = {
  sceneId: string
  baseUrl: string
  baseUrlBundles: string
  name: string
  mappingsResponse: {
    parcel_id: string
    contents: Mapping[]
    root_cid: string
    publisher: string
  }
  scene: {
    display?: {
      title: string
    }
    scene: {
      base?: string
      parcels: string[]
    }
    spawnPoints?: {
      name?: string
      position: {
        x: number | number[]
        y: number | number[]
        z: number | number[]
      }
      cameraTarget?: any
      default?: boolean
    }[]
  }
}
export type SceneInfoState = ValidSceneState
export type ValidSceneState = {
  // Used to know what scene id corresponds to each parcel position
  positionToSceneId: Record<StringCoordinate, SceneId>
  // ... and its reverse mapping
  sceneIdToPositions: Record<SceneId, StringCoordinate[]>
  // Contents
  sceneIdToMappings: Record<SceneId, Mapping[]>
  // Metadata
  sceneIdToSceneJson: Record<SceneId, SceneMetadata>
}
export type SceneContentServerEntity = {
  id: SceneId
  type: string
  pointers: StringCoordinate[]
  content: Mapping[]
  metadata: SceneMetadata
}
export const SCENE_INITIAL_STATE: ValidSceneState = {
  positionToSceneId: {},
  sceneIdToPositions: {},
  sceneIdToMappings: {},
  sceneIdToSceneJson: {}
}
