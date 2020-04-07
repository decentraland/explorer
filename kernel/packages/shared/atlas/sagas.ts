import { Vector2Component } from 'atomicHelpers/landHelpers'
import { MinimapSceneInfo } from 'decentraland-ecs/src/decentraland/Types'
// @ts-ignore
import { call, fork, put, select, take, takeEvery, all, race } from 'redux-saga/effects'
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
  reportScenesAroundParcel
} from './actions'
import { shouldLoadSceneJsonData } from './selectors'
import { AtlasState, MARKET_DATA, QUERY_DATA_FROM_SCENE_JSON, REPORT_SCENES_AROUND_PARCEL, SUCCESS_DATA_FROM_SCENE_JSON, FAILURE_DATA_FROM_SCENE_JSON } from './types'
import { getTilesRectFromCenter } from '../utils'
import { Action } from 'redux'
import { ILand } from 'shared/types'

declare const window: {
  unityInterface: {
    UpdateMinimapSceneInformation: (data: MinimapSceneInfo[]) => void
  }
}

export function* atlasSaga(): any {
  yield fork(fetchDistricts)
  yield fork(fetchTiles)

  yield takeEvery(QUERY_DATA_FROM_SCENE_JSON, querySceneDataAction)
  yield takeEvery(REPORT_SCENES_AROUND_PARCEL, reportScenesAroundParcelAction)
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

export function* checkAndReportAround() {
  const userPosition = lastPlayerPosition
  let lastReport: Vector2Component = yield select(state => state.atlas.lastReportPosition)
  const TRIGGER_DISTANCE = 10 * parcelLimits.parcelSize
  const MAX_SCENES_AROUND = 15

  if (
    Math.abs(userPosition.x - lastReport.x) > TRIGGER_DISTANCE ||
    Math.abs(userPosition.z - lastReport.y) > TRIGGER_DISTANCE
  ) {
    const userPosition = lastPlayerPosition
    const userX = userPosition.x / parcelLimits.parcelSize
    const userY = userPosition.z / parcelLimits.parcelSize
    yield put(reportScenesAroundParcel({ x: userX, y: userY }, MAX_SCENES_AROUND))
  }
}

function isSceneAction(type: string, sceneId: string) {
  return (action: Action) => type === action.type && sceneId === (action as any)?.payload?.sceneId
}

export function* reportScenesAroundParcelAction(action: ReportScenesAroundParcel) {
  let atlasState: AtlasState = yield select(state => state.atlas)

  while (!atlasState.hasMarketData) {
    yield take(MARKET_DATA)
    atlasState = yield select(state => state.atlas)
  }

  const tilesAround = getTilesRectFromCenter(action.payload.parcelCoord, action.payload.scenesAround)

  const sceneIds: (string | null)[] = yield call(fetchSceneIds, tilesAround)

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

  // renew atlas state to get loaded scenes information
  atlasState = yield select(state => state.atlas)

  yield call(reportScenes, atlasState, [...sceneIdsSet])
}

export function* reportScenes(atlas?: AtlasState, sceneIds: string[] = []): any {
  if (!atlas) {
    return
  }

  const scenes = sceneIds.map(sceneId => atlas.idToScene[sceneId])

  const minimapSceneInfoResult: MinimapSceneInfo[] = []

  scenes.forEach(scene => {
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

  defaultLogger.info(`minimap`, minimapSceneInfoResult)

  window.unityInterface.UpdateMinimapSceneInformation(minimapSceneInfoResult)
}
