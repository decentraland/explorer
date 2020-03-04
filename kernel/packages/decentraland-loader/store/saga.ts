import { call, put, select, takeEvery, takeLatest } from 'redux-saga/effects'
import defaultLogger from 'shared/logger'
import {
  BOOTSTRAP_AT,
  downloadEnqueuedParcels,
  enqueueUnknownParcels,
  ENQUEUE_UNKNOWN_PARCELS,
  loadScene,
  LOAD_SCENE,
  processParcelSightChange,
  processSceneSightChange,
  processUserMovement,
  PROCESS_PARCEL_SIGHT_CHANGE,
  PROCESS_USER_MOVEMENT,
  PROCESS_USER_TELEPORT,
  ResolveSpawnPosition,
  resolveSpawnPosition,
  resolveToEmptyParcel,
  RESOLVE_SPAWN_POSITION,
  startRendering,
  START_SCENE,
  stopScene,
  storeResolvedSceneEntity,
  STORE_RESOLVED_SCENE_ENTITY
} from './actions'
import { fetchScenesFromServer } from './download/fetchV3'
import { getEmptyEntity } from './overrides/emptyParcels'
import { getTutorialEntity } from './overrides/tutorial'
import { SceneContentServerEntity } from './sceneInfo/types'
import { UNLOADED } from './sceneSight/sceneStatus'
import { shouldSelectSpawnTarget } from './selectors/canSpawn'
import { canStartRendering } from './selectors/canStartRendering'
import { filterNotResolvedParcels } from './selectors/filterNotResolvedParcels'
import { generateSightDeltaFromState } from './selectors/generateSightMapFromState'
import { getCurrentSceneEntity } from './selectors/getCurrentScene'
import { getAllScenes } from './selectors/getSceneStatus'
import { pickWorldSpawnpoint } from './selectors/getSpawnPosition'
import { unknownParcels } from './selectors/unknownParcels'
import { RootState } from './state'
import { stringCoordinateToXY, worldToStringCoordinate } from './utils/worldToGrid'

export function* rootSaga() {
  const interruptableEffectHandlers: [string, ((action: any) => Generator<any> | (() => Generator<any>))][] = [
    [BOOTSTRAP_AT, triggerFirstEnqueue],
    [RESOLVE_SPAWN_POSITION, triggerUserMovement],
    [PROCESS_USER_TELEPORT, triggerParcelSightChange],
    [PROCESS_USER_MOVEMENT, triggerParcelSightChangeOnUserMovement],
    [PROCESS_PARCEL_SIGHT_CHANGE, triggerEnqueueUnknown],
    [PROCESS_PARCEL_SIGHT_CHANGE, triggerSceneSightChange],
    [STORE_RESOLVED_SCENE_ENTITY, triggerSceneSightChange],
    [RESOLVE_SPAWN_POSITION, triggerStartRendering]
  ]
  const parallelEffectHandlers: [string, (action: any) => Generator<any>][] = [
    [ENQUEUE_UNKNOWN_PARCELS, triggerSceneResolution],
    [LOAD_SCENE, triggerSelectSpawnPosition],
    [START_SCENE, triggerStartRendering]
  ]
  for (let [action, trigger] of interruptableEffectHandlers) {
    try {
      yield takeLatest(action, trigger)
    } catch (error) {
      defaultLogger.error(`Saga ${trigger.name} (taking ${action}) execution error:`, error)
      yield select(state => defaultLogger.log(`Error ocurred when state was:`, state))
    }
  }
  for (let [action, trigger] of parallelEffectHandlers) {
    try {
      yield takeEvery(action, trigger)
    } catch (error) {
      defaultLogger.error(`Saga ${trigger.name} (taking ${action}) execution error:`, error)
      yield select(state => defaultLogger.log(`Error ocurred when state was:`, state))
    }
  }
}

export function* triggerSelectSpawnPosition() {
  if (yield select(shouldSelectSpawnTarget)) {
    const currentScene = (yield select(getCurrentSceneEntity)) as ReturnType<typeof getCurrentSceneEntity>
    const targetPosition = pickWorldSpawnpoint(currentScene.metadata.scene)
    yield put(resolveSpawnPosition(targetPosition.position, targetPosition.cameraTarget))
  }
}

export function* triggerFirstEnqueue() {
  yield put(storeResolvedSceneEntity(getTutorialEntity()))
  yield put(enqueueUnknownParcels())
}

export function* triggerUserMovement(action: ResolveSpawnPosition) {
  const position = worldToStringCoordinate(action.payload.position)
  yield put(processUserMovement(stringCoordinateToXY(position)))
}

export function* triggerEnqueueUnknown() {
  yield put(enqueueUnknownParcels())
}

export function* triggerParcelSightChangeOnUserMovement() {
  if (
    yield select(
      (state: RootState) =>
        state.position.previousPosition && state.position.previousPosition !== state.position.currentPosition
    )
  ) {
    yield put(processParcelSightChange())
  }
}

export function* triggerParcelSightChange() {
  yield put(processParcelSightChange())
}

export function* triggerStartRendering(): any {
  if (yield select(canStartRendering)) {
    yield put(startRendering())
  }
}
function getConfigurationFromState(state: RootState) {
  return state.configuration
}

export function* triggerSceneResolution() {
  const parcels = (yield select(unknownParcels)) as ReturnType<typeof unknownParcels>
  if (!parcels.length) {
    return
  }
  yield put(downloadEnqueuedParcels())
  const config = (yield select(getConfigurationFromState)) as ReturnType<typeof getConfigurationFromState>
  const results: SceneContentServerEntity[] = yield call(fetchScenesFromServer, parcels, config)
  for (let entity of results) {
    yield put(storeResolvedSceneEntity(entity))
  }
  const empty = yield select(filterNotResolvedParcels, parcels)
  for (let parcel of empty) {
    const emptyScene = getEmptyEntity(parcel, config)
    yield put(storeResolvedSceneEntity(emptyScene))
  }
  yield put(resolveToEmptyParcel(empty))
}

export function* triggerSceneSightChange() {
  const deltaScenes = (yield select(generateSightDeltaFromState)) as ReturnType<typeof generateSightDeltaFromState>
  const sceneStatus = (yield select(getAllScenes)) as ReturnType<typeof getAllScenes>
  for (let scene of Object.keys(deltaScenes.currentSight)) {
    if (!sceneStatus[scene] || sceneStatus[scene] === UNLOADED) {
      yield put(loadScene(scene))
    }
  }
  for (let scene of deltaScenes.lostSight) {
    if (sceneStatus[scene] !== UNLOADED) {
      yield put(stopScene(scene))
    }
  }
  yield put(processSceneSightChange(deltaScenes.currentSight))
}
