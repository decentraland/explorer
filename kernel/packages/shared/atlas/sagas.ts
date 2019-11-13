import { call, fork, put, select, takeEvery, takeLatest } from 'redux-saga/effects'
import { getServer, LifecycleManager } from '../../decentraland-loader/lifecycle/manager'
import { SceneStart, SCENE_START } from '../loading/actions'
import {
  districtData,
  fetchNameFromSceneJson,
  fetchNameFromSceneJsonFailure,
  fetchNameFromSceneJsonSuccess,
  FetchNameFromSceneJsonSuccess,
  marketData,
  MarketDataAction,
  QuerySceneName
} from './actions'
import { getNameFromAtlasState, shouldLoadSceneJsonName } from './selectors'
import { FETCH_NAME_FROM_SCENE_JSON, MARKET_DATA, SUCCESS_NAME_FROM_SCENE_JSON } from './types'

export function* atlasSaga(): any {
  yield fork(fetchDistricts)
  yield fork(fetchTiles)

  yield takeEvery(SCENE_START, querySceneName)
  yield takeEvery(FETCH_NAME_FROM_SCENE_JSON, fetchName)

  yield takeLatest(MARKET_DATA, reportAll)
  yield takeLatest(SUCCESS_NAME_FROM_SCENE_JSON, reportOne)
}

function* fetchDistricts() {
  try {
    const districts = yield call(() => fetch('https://api.decentraland.org/v1/districts').then(e => e.json()))
    yield put(districtData(districts))
  } catch (e) {
    console.log(e)
  }
}
function* fetchTiles() {
  try {
    const tiles = yield call(() => fetch('https://api.decentraland.org/v1/tiles').then(e => e.json()))
    yield put(marketData(tiles))
  } catch (e) {
    console.log(e)
  }
}

function* querySceneName(action: QuerySceneName) {
  if (yield select(shouldLoadSceneJsonName, action.payload) !== undefined) {
    yield put(fetchNameFromSceneJson(action.payload))
  }
}

function* fetchName(action: SceneStart) {
  try {
    const { name, parcels } = yield call(() => getNameFromSceneJson(action.payload))
    yield put(fetchNameFromSceneJsonSuccess(action.payload, name, parcels))
  } catch (e) {
    yield put(fetchNameFromSceneJsonFailure(action.payload, e))
  }
}

async function getNameFromSceneJson(sceneId: string) {
  const server: LifecycleManager = getServer()

  const land = (await server.getParcelData(sceneId)) as any
  return { name: land.scene.display.title, parcels: land.scene.scene.parcels }
}

function* reportOne(action: FetchNameFromSceneJsonSuccess) {
  const atlasState = yield select(state => state.atlas)
  const parcels = action.payload.parcels
  const unity = (window as any)['unityInterface'] as any
  unity.UpdateMinimapSceneNames(
    parcels.map(p => {
      const [x, y] = p.split(',').map(_ => parseInt(_, 10))
      const name = getNameFromAtlasState(atlasState, x, y)
      return { x, y, name }
    })
  )
}
function* reportAll(action: MarketDataAction) {
  const atlasState = yield select(state => state.atlas)
  const data = action.payload.data
  const unity = (window as any)['unityInterface'] as any
  unity.UpdateMinimapSceneInformation(
    Object.keys(data).map((x: any) => ({
      x: data[x].x,
      y: data[x].y,
      name: getNameFromAtlasState(atlasState, data[x].x, data[x].y),
      type: data[x].type
    }))
  )
}
