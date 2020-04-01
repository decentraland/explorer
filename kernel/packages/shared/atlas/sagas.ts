import { Vector2Component } from 'atomicHelpers/landHelpers'
import { MinimapSceneInfo } from 'decentraland-ecs/src/decentraland/Types'
import { call, fork, put, putResolve, select, take, takeEvery, all } from 'redux-saga/effects'
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
import { AtlasState, MapSceneData, MARKET_DATA, QUERY_DATA_FROM_SCENE_JSON, REPORT_SCENES_AROUND_PARCEL } from './types'
import { getTilesRectFromCenter } from '../utils'

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
  if (yield select(shouldLoadSceneJsonData, action.payload) !== undefined) {
    yield call(fetchSceneJsonData, action.payload)
  }
}

function* fetchSceneJsonData(sceneId: string) {
  try {
    const land = yield call(() => fetchSceneJson(sceneId))
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

async function fetchSceneIds(center: Vector2Component, size: number) {
  const server: LifecycleManager = getServer()
  const id = await server.getSceneIds(center, size)
  defaultLogger.log(`fetching position end! ${center} ... id = ${id}`)
  return id
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

export function* reportScenesAroundParcelAction(action: ReportScenesAroundParcel) {
  let atlasState = (yield select(state => state.atlas)) as AtlasState

  while (!atlasState.hasMarketData) {
    defaultLogger.log('waiting for market data...')
    yield take(MARKET_DATA)
    atlasState = yield select(state => state.atlas)
  }

  const tilesAround = getTilesRectFromCenter(action.payload.parcelCoord, action.payload.scenesAround)
  let sceneIdsSet: Set<string> = new Set<string>()

  defaultLogger.log(`waiting for ids... ${JSON.stringify(tilesAround)}`)

  const sceneIds: string[] = yield call(() => fetchSceneIds(action.payload.parcelCoord, action.payload.scenesAround))

  for (let id of sceneIds) {
    sceneIdsSet.add(id)
  }

  defaultLogger.log(`waiting for scenes... ${JSON.stringify(sceneIdsSet)}`)

  for (let id of sceneIdsSet) {
    yield putResolve(querySceneData(id))
  }

  defaultLogger.log(`end!`)

  yield call(reportScenes, atlasState, tilesAround)
}

export function* reportScenes(atlas?: AtlasState, tiles?: string[]): any {
  //NOTE(Brian): Check unique scenes inside tiles array argument
  let scenes: Set<MapSceneData> = new Set<MapSceneData>()

  tiles?.forEach(x => {
    if (!atlas) {
      return
    }

    const scene = atlas.tileToScene[x]
    if (!scenes.has(scene)) {
      scenes.add(scene)
    }
  })

  //NOTE(Brian): iterate unique scenes, fill up the minimapSceneInfo and send it over
  //             the update message to renderer.
  let minimapSceneInfoResult: MinimapSceneInfo[] = []

  scenes.forEach(scene => {
    let parcels: Vector2Component[] = []
    let isPOI: boolean = false

    scene.sceneJsonData?.scene.parcels.forEach(p => {
      let xyStr = p.split(',')
      let xy: Vector2Component = { x: parseInt(xyStr[0], 10), y: parseInt(xyStr[1], 10) }

      if (CAMPAIGN_PARCEL_SEQUENCE.includes({ x: xy.x, y: xy.y })) {
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
      parcels: parcels,
      isPOI: isPOI
    } as MinimapSceneInfo)
  })

  window.unityInterface.UpdateMinimapSceneInformation(minimapSceneInfoResult)
}
