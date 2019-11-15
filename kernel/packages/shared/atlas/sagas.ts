import { call, fork, put, select, takeEvery, takeLatest } from 'redux-saga/effects'
import { getServer, LifecycleManager } from '../../decentraland-loader/lifecycle/manager'
import { SceneStart, SCENE_START } from '../loading/actions'
import { RENDERER_INITIALIZED } from '../renderer/types'
import {
  districtData,
  fetchNameFromSceneJson,
  fetchNameFromSceneJsonFailure,
  fetchNameFromSceneJsonSuccess,
  FetchNameFromSceneJsonSuccess,
  marketData,
  QuerySceneName
} from './actions'
import { getNameFromAtlasState, getTypeFromAtlasState, shouldLoadSceneJsonName } from './selectors'
import { AtlasState, FETCH_NAME_FROM_SCENE_JSON, SUCCESS_NAME_FROM_SCENE_JSON } from './types'
import defaultLogger from '../logger'

declare const window: {
  unityInterface: {
    UpdateMinimapSceneInformation: (data: { name: string; type: number; parcels: number[][] }[]) => void
  }
}

export function* atlasSaga(): any {
  yield fork(fetchDistricts)
  yield fork(fetchTiles)

  yield takeEvery(SCENE_START, querySceneName)
  yield takeEvery(FETCH_NAME_FROM_SCENE_JSON, fetchName)

  yield takeLatest(RENDERER_INITIALIZED, reportAll)
  yield takeLatest(SUCCESS_NAME_FROM_SCENE_JSON, reportOne)
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
  const [firstX, firstY] = parcels[0].split(',').map(_ => parseInt(_, 10))
  const name = getNameFromAtlasState(atlasState, firstX, firstY)
  const type = getTypeFromAtlasState(atlasState, firstX, firstY)
  window.unityInterface.UpdateMinimapSceneInformation([
    {
      name,
      type,
      parcels: parcels.map(p => {
        const [x, y] = p.split(',').map(_ => parseInt(_, 10))
        return [x, y]
      })
    }
  ])
}
function* reportAll() {
  const atlasState = (yield select(state => state.atlas)) as AtlasState
  const data = atlasState.marketName
  const mapByTypeAndName: Record<string, number[][]> = {}
  const typeAndNameKeys: string[] = []
  const keyToTypeAndName: Record<string, { type: number; name: string }> = {}
  Object.keys(data).forEach(index => {
    const parcel = data[index]
    const name = getNameFromAtlasState(atlasState, parcel.x, parcel.y)
    const type = getTypeFromAtlasState(atlasState, parcel.x, parcel.y)
    const key = `${type}_${name}`
    if (!mapByTypeAndName[key]) {
      mapByTypeAndName[key] = []
      typeAndNameKeys.push(key)
      keyToTypeAndName[key] = { type, name }
    }
    mapByTypeAndName[key].push([parcel.x, parcel.y])
  })
  window.unityInterface.UpdateMinimapSceneInformation(
    typeAndNameKeys.map(key => ({
      name: keyToTypeAndName[key].name,
      type: keyToTypeAndName[key].type,
      parcels: mapByTypeAndName[key]
    }))
  )
}
