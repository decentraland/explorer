import { action } from 'typesafe-actions'
import { ConfigState } from './config/config'
import { SceneContentServerEntity, SceneId, StringCoordinate } from './sceneInfo/types'

type Vector3 = { x: number; y: number; z: number }
type Vector2 = { x: number; y: number }

export const BOOTSTRAP_AT = 'Bootstrapping loader state'
export const bootstrapAt = (coordinate: StringCoordinate, config?: ConfigState) =>
  action(BOOTSTRAP_AT, { coordinate, config })
export type Bootstrap = ReturnType<typeof bootstrapAt>

export const ENQUEUE_UNKNOWN_PARCELS = 'Enqueue parcels needing sceneId resolution'
export const enqueueUnknownParcels = () => action(ENQUEUE_UNKNOWN_PARCELS)
export type EnqueueUnknownParcels = ReturnType<typeof enqueueUnknownParcels>

export const DOWNLOAD_ENQUEUED_PARCELS = 'Download enqueued parcels'
export const downloadEnqueuedParcels = () => action(DOWNLOAD_ENQUEUED_PARCELS)
export type DownloadEnqueuedParcels = ReturnType<typeof downloadEnqueuedParcels>

export const STORE_RESOLVED_SCENE_ENTITY = 'Store resolved scene entity'
export const storeResolvedSceneEntity = (sceneEntity: SceneContentServerEntity) =>
  action(STORE_RESOLVED_SCENE_ENTITY, { sceneEntity })
export type StoreResolvedSceneEntity = ReturnType<typeof storeResolvedSceneEntity>

export const RESOLVE_TO_EMPTY_PARCEL = 'Resolve to empty parcel'
export const resolveToEmptyParcel = (emptyParcels: StringCoordinate[]) =>
  action(RESOLVE_TO_EMPTY_PARCEL, { emptyParcels })
export type ResolveToEmptyParcel = ReturnType<typeof resolveToEmptyParcel>

export const RESOLVE_SPAWN_POSITION = 'Resolve spawn position'
export const resolveSpawnPosition = (position: Vector3, cameraTarget?: Vector3) =>
  action(RESOLVE_SPAWN_POSITION, { position, cameraTarget })
export type ResolveSpawnPosition = ReturnType<typeof resolveSpawnPosition>

export const PROCESS_USER_MOVEMENT = 'Process user movement'
export const processUserMovement = (newPosition: Vector2) => action(PROCESS_USER_MOVEMENT, { newPosition })
export type ProcessUserMovement = ReturnType<typeof processUserMovement>

export const PROCESS_USER_TELEPORT = 'Teleport user'
export const processUserTeleport = (target: StringCoordinate) => action(PROCESS_USER_TELEPORT, { target })
export type ProcessUserTeleport = ReturnType<typeof processUserTeleport>

export const PROCESS_PARCEL_SIGHT_CHANGE = 'Process changes in sighted parcels'
export const processParcelSightChange = () => action(PROCESS_PARCEL_SIGHT_CHANGE)
export type ProcessParcelSightChange = ReturnType<typeof processParcelSightChange>

export const PROCESS_SCENE_SIGHT_CHANGE = 'Process changes in sighted scenes'
export const processSceneSightChange = (newValue: Record<SceneId, number>) =>
  action(PROCESS_SCENE_SIGHT_CHANGE, { newValue })
export type ProcessSceneSightChange = ReturnType<typeof processSceneSightChange>

export const LOAD_SCENE = 'Load scene'
export const loadScene = (sceneId: SceneId) => action(LOAD_SCENE, sceneId)
export type LoadScene = ReturnType<typeof loadScene>

export const START_SCENE = 'Start scene'
export const startScene = (sceneId: SceneId) => action(START_SCENE, sceneId)
export type StartScene = ReturnType<typeof startScene>

export const START_RENDERING = 'Start rendering'
export const startRendering = () => action(START_RENDERING)
export type TriggerSettle = ReturnType<typeof startRendering>

export const STOP_SCENE = 'Stop scene'
export const stopScene = (sceneId: SceneId) => action(STOP_SCENE, sceneId)
export type StopScene = ReturnType<typeof stopScene>

export const CONFIGURE = 'Configure'
export const configure = (config: ConfigState) => action(CONFIGURE, config)
export type Configure = ReturnType<typeof configure>

export type LoaderAction =
  | Bootstrap
  | EnqueueUnknownParcels
  | DownloadEnqueuedParcels
  | StoreResolvedSceneEntity
  | ResolveToEmptyParcel
  | ResolveSpawnPosition
  | ProcessUserMovement
  | ProcessUserTeleport
  | ProcessParcelSightChange
  | ProcessSceneSightChange
  | Configure
  | TriggerSettle
  | LoadScene
  | StartScene
  | StopScene

export type ActionTypes =
  | typeof BOOTSTRAP_AT
  | typeof ENQUEUE_UNKNOWN_PARCELS
  | typeof DOWNLOAD_ENQUEUED_PARCELS
  | typeof STORE_RESOLVED_SCENE_ENTITY
  | typeof RESOLVE_TO_EMPTY_PARCEL
  | typeof RESOLVE_SPAWN_POSITION
  | typeof PROCESS_USER_MOVEMENT
  | typeof PROCESS_USER_TELEPORT
  | typeof PROCESS_PARCEL_SIGHT_CHANGE
  | typeof PROCESS_SCENE_SIGHT_CHANGE
  | typeof CONFIGURE
  | typeof START_RENDERING
  | typeof LOAD_SCENE
  | typeof START_SCENE
  | typeof STOP_SCENE

export const actions = [
  BOOTSTRAP_AT,
  ENQUEUE_UNKNOWN_PARCELS,
  DOWNLOAD_ENQUEUED_PARCELS,
  STORE_RESOLVED_SCENE_ENTITY,
  RESOLVE_TO_EMPTY_PARCEL,
  RESOLVE_SPAWN_POSITION,
  PROCESS_USER_MOVEMENT,
  PROCESS_USER_TELEPORT,
  PROCESS_PARCEL_SIGHT_CHANGE,
  PROCESS_SCENE_SIGHT_CHANGE,
  CONFIGURE,
  START_RENDERING,
  LOAD_SCENE,
  START_SCENE,
  STOP_SCENE
]
