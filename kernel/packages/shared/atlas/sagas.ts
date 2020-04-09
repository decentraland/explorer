import { Vector2Component } from 'atomicHelpers/landHelpers'
import { MinimapSceneInfo } from 'decentraland-ecs/src/decentraland/Types'
import { call, fork, put, select, take, takeEvery, race, takeLatest } from 'redux-saga/effects'
import { CAMPAIGN_PARCEL_SEQUENCE } from 'shared/world/TeleportController'
import { parcelLimits } from '../../config'
import { getServer, LifecycleManager } from '../../decentraland-loader/lifecycle/manager'
import { getOwnerNameFromJsonData, getSceneDescriptionFromJsonData } from '../../shared/selectors'
import defaultLogger from '../logger'
import { lastPlayerPosition } from '../world/positionThings'
import {
  districtData,
  fetchDataFromSceneJsonFailure,
  fetchDataFromSceneJsonSuccess,
  marketData,
  querySceneData,
  QuerySceneData,
  ReportScenesAroundParcel,
  reportedScenes,
  QUERY_DATA_FROM_SCENE_JSON,
  REPORT_SCENES_AROUND_PARCEL,
  MARKET_DATA,
  SUCCESS_DATA_FROM_SCENE_JSON,
  FAILURE_DATA_FROM_SCENE_JSON,
  reportScenesAroundParcel,
  reportLastPosition
} from './actions'
import { shouldLoadSceneJsonData, isMarketDataInitialized } from './selectors'
import { AtlasState } from './types'
import { getTilesRectFromCenter } from '../utils'
import { Action } from 'redux'
import { ILand } from 'shared/types'
import { SCENE_LOAD } from 'shared/loading/actions'
import { worldToGrid } from '../../atomicHelpers/parcelScenePositions';

declare const window: {
  unityInterface: {
    UpdateMinimapSceneInformation: (data: MinimapSceneInfo[]) => void
  }
}

export function* atlasSaga(): any {
  yield fork(fetchDistricts)
  yield fork(fetchTiles)

  yield takeEvery(SCENE_LOAD, checkAndReportAround)

  yield takeEvery(QUERY_DATA_FROM_SCENE_JSON, querySceneDataAction)
  yield takeLatest(REPORT_SCENES_AROUND_PARCEL, reportScenesAroundParcelAction)
}

function* fetchDistricts() {
  try {
    const districts = yield call(() => fetch('https://api.decentraland.org/v1/districts').then(e => e.json()))
    yield put(districtData(districts))
  } catch (e) {
    defaultLogger.log(e)
  }
}

function* fetchTiles() {
  try {
    const tiles = yield call(() => fetch('https://api.decentraland.org/v1/tiles').then(e => e.json()))
    yield put(marketData(tiles))
  } catch (e) {
    defaultLogger.log(e)
  }
}

function* querySceneDataAction(action: QuerySceneData) {
  const shouldFetch = yield select(shouldLoadSceneJsonData, action.payload)
  if (shouldFetch) {
    yield call(fetchSceneJsonData, action.payload)
  }
}

function* fetchSceneJsonData(sceneId: string) {
  try {
    const land: ILand = yield call(fetchSceneJson, sceneId)
    yield put(fetchDataFromSceneJsonSuccess(sceneId, land))
  } catch (e) {
    yield put(fetchDataFromSceneJsonFailure(sceneId, e))
  }
}

async function fetchSceneJson(sceneId: string) {
  const server: LifecycleManager = getServer()
  const land = await server.getParcelData(sceneId)
  return land
}

async function fetchSceneIds(tiles: string[]) {
  const server: LifecycleManager = getServer()
  const promises = server.getSceneIds(tiles)
  return Promise.all(promises)
}

const TRIGGER_DISTANCE = 10 * parcelLimits.parcelSize
const MAX_SCENES_AROUND = 15

export function* checkAndReportAround() {
  const userPosition = lastPlayerPosition
  const lastReport: Vector2Component | undefined = yield select(state => state.atlas.lastReportPosition)

  if (
    !lastReport ||
    Math.abs(userPosition.x - lastReport.x) > TRIGGER_DISTANCE ||
    Math.abs(userPosition.z - lastReport.y) > TRIGGER_DISTANCE
  ) {
    const gridPosition = worldToGrid(userPosition)

    yield put(reportScenesAroundParcel(gridPosition, MAX_SCENES_AROUND))
    yield put(reportLastPosition({ x: userPosition.x, y: userPosition.z }))
  }
}

function isSceneAction(type: string, sceneId: string) {
  return (action: Action) => type === action.type && sceneId === (action as any)?.payload?.sceneId
}

export function* reportScenesAroundParcelAction(action: ReportScenesAroundParcel) {
  let marketDataInitialized: boolean = yield select(isMarketDataInitialized)

  while (!marketDataInitialized) {
    yield take(MARKET_DATA)
    marketDataInitialized = yield select(isMarketDataInitialized)
  }

  const tilesAround = getTilesRectFromCenter(action.payload.parcelCoord, MAX_SCENES_AROUND)

  defaultLogger.info(`atlas#tiles-around`, tilesAround)

  const sceneIds: (string | null)[] = yield call(fetchSceneIds, tilesAround)

  defaultLogger.info(`atlas#scene-array`, sceneIds)

  const sceneIdsSet = new Set<string>(sceneIds.filter($ => $ !== null) as string[])

  for (const id of sceneIdsSet) {
    yield put(querySceneData(id))
  }

  for (const id of sceneIdsSet) {
    const shouldFetch = yield select(shouldLoadSceneJsonData, id)
    if (shouldFetch) {
      yield race({
        success: take(isSceneAction(SUCCESS_DATA_FROM_SCENE_JSON, id)),
        failure: take(isSceneAction(FAILURE_DATA_FROM_SCENE_JSON, id))
      })
    }
  }

  yield call(reportScenes, [...sceneIdsSet])
  yield put(reportedScenes(tilesAround))
}

function* reportScenes(sceneIds: string[]): any {
  const atlas: AtlasState = yield select(state => state.atlas)

  const scenes = sceneIds.map(sceneId => atlas.idToScene[sceneId])

  const minimapSceneInfoResult: MinimapSceneInfo[] = []

  scenes
    .filter(scene => !scene.alreadyReported)
    .forEach(scene => {
      const parcels: Vector2Component[] = []
      let isPOI: boolean = false

      scene.sceneJsonData?.scene.parcels.forEach(parcel => {
        let xyStr = parcel.split(',')
        let xy: Vector2Component = { x: parseInt(xyStr[0], 10), y: parseInt(xyStr[1], 10) }

        if (CAMPAIGN_PARCEL_SEQUENCE.some(poi => poi.x === xy.x && poi.y === xy.y)) {
          isPOI = true
        }

        parcels.push(xy)
      })

      minimapSceneInfoResult.push({
        owner: getOwnerNameFromJsonData(scene.sceneJsonData),
        description: getSceneDescriptionFromJsonData(scene.sceneJsonData),
        previewImageUrl: scene.sceneJsonData?.display?.navmapThumbnail,
        name: scene.name,
        type: scene.type,
        parcels,
        isPOI
      })
    })

  window.unityInterface.UpdateMinimapSceneInformation(minimapSceneInfoResult)
}
