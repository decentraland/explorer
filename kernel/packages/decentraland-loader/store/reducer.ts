import {
  BOOTSTRAP_AT,
  CONFIGURE,
  DOWNLOAD_ENQUEUED_PARCELS,
  ENQUEUE_UNKNOWN_PARCELS,
  LoaderAction,
  LOAD_SCENE,
  PROCESS_PARCEL_SIGHT_CHANGE,
  PROCESS_SCENE_SIGHT_CHANGE,
  PROCESS_USER_MOVEMENT,
  PROCESS_USER_TELEPORT,
  RESOLVE_SPAWN_POSITION,
  RESOLVE_TO_EMPTY_PARCEL,
  START_RENDERING,
  START_SCENE,
  STOP_SCENE,
  STORE_RESOLVED_SCENE_ENTITY
} from './actions'
import { ConfigState, updateConfig } from './config/config'
import {
  DOWNLOAD_INITIAL_STATE,
  enqueueValues,
  markAsEmpty,
  markAsKnown,
  markAsPending
} from './download/downloadState'
import {
  initialPositionState,
  updateStartRendering,
  updateStateOnSpawn,
  updateStateOnTeleport,
  updateStateOnUserMovement
} from './position/positionState'
import { clearRecent, initialSight, sightChange } from './position/sightInfo'
import { incorporateLandInfo } from './sceneInfo/sceneInfo'
import { SCENE_INITIAL_STATE } from './sceneInfo/types'
import { LOADING, STARTED, UNLOADED } from './sceneSight/sceneStatus'
import { unknownParcels } from './selectors/unknownParcels'
import { RootState } from './state'

const DEFAULT_CONFIG: ConfigState = {
  contentServer: 'https://content.decentraland.org',
  contentServerBundles: 'https://content-assets-as-bundle.decentraland.org',
  emptyScenes: true,
  lineOfSightRadius: 4,
  secureRadius: 4,
  tutorialBaseURL: '',
  tutorialSceneEnabled: false
}

export function rootReducer(state?: RootState, action?: LoaderAction): RootState {
  if (!state) {
    const coordinate = action ? (action.type === BOOTSTRAP_AT ? action.payload.coordinate : '0,0') : '0,0'
    const config = (action && (action as any).payload && (action as any).payload.config) || DEFAULT_CONFIG
    return {
      configuration: config,
      download: DOWNLOAD_INITIAL_STATE,
      position: initialPositionState(coordinate),
      sceneInfo: SCENE_INITIAL_STATE,
      sightInfo: initialSight(coordinate),
      sceneSight: {},
      sceneState: {}
    }
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case BOOTSTRAP_AT:
      return rootReducer(undefined, action)
    case CONFIGURE:
      return {
        ...state,
        configuration: updateConfig(state.configuration, action.payload)
      }
    case START_RENDERING:
      return {
        ...state,
        position: updateStartRendering(state.position)
      }
    case ENQUEUE_UNKNOWN_PARCELS:
      return {
        ...state,
        download: enqueueValues(state.download, unknownParcels(state))
      }
    case DOWNLOAD_ENQUEUED_PARCELS:
      return {
        ...state,
        download: markAsPending(state.download, state.download.queued),
        sightInfo: clearRecent(state.sightInfo)
      }
    case STORE_RESOLVED_SCENE_ENTITY:
      return {
        ...state,
        download: markAsKnown(state.download, action.payload.sceneEntity),
        sceneInfo: incorporateLandInfo(state.sceneInfo, action.payload.sceneEntity)
      }
    case PROCESS_USER_TELEPORT:
      const newPosition = updateStateOnTeleport(state.position, action.payload.target)
      return {
        ...state,
        position: newPosition,
        sightInfo: sightChange(state.sightInfo, action.payload.target)
      }
    case PROCESS_USER_MOVEMENT:
      return {
        ...state,
        position: updateStateOnUserMovement(state.position, action.payload.newPosition)
      }
    case PROCESS_SCENE_SIGHT_CHANGE:
      return {
        ...state,
        sceneSight: action.payload.newValue
      }
    case PROCESS_PARCEL_SIGHT_CHANGE:
      return {
        ...state,
        sightInfo: sightChange(state.sightInfo, state.position.targetPosition)
      }
    case RESOLVE_SPAWN_POSITION:
      return {
        ...state,
        position: updateStateOnSpawn(state.position, action.payload.position, action.payload.cameraTarget!)
      }
    case RESOLVE_TO_EMPTY_PARCEL:
      return {
        ...state,
        download: markAsEmpty(state.download, action.payload.emptyParcels)
      }
    case LOAD_SCENE:
      return {
        ...state,
        sceneState: {
          ...state.sceneState,
          [action.payload]: LOADING
        }
      }
    case START_SCENE:
      return {
        ...state,
        sceneState: {
          ...state.sceneState,
          [action.payload]: STARTED
        }
      }
    case STOP_SCENE:
      return {
        ...state,
        sceneState: {
          ...state.sceneState,
          [action.payload]: UNLOADED
        }
      }
    default:
      return state
  }
}
