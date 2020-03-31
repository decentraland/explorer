import { AnyAction } from 'redux'
import { REPORTED_SCENES_FOR_MINIMAP } from './actions'
import {
  AtlasState,
  District,
  DISTRICT_DATA,
  FAILURE_DATA_FROM_SCENE_JSON,
  FETCH_DATA_FROM_SCENE_JSON,
  MarketData,
  MARKET_DATA,
  SUCCESS_DATA_FROM_SCENE_JSON,
  MapSceneData
} from './types'
import { ILand } from 'shared/types'
import { getNameFromAtlasState, getTypeFromAtlasState } from './selectors'
import { Vector2Component } from 'atomicHelpers/landHelpers'

const ATLAS_INITIAL_STATE: AtlasState = {
  hasMarketData: false,
  hasDistrictData: false,
  scenes: new Set<MapSceneData>(),
  tileToScene: {}, // '0,0' -> sceneId. Useful for mapping tile market data to actual scenes.
  idToScene: {}, // sceneId -> MapScene
  lastReportPosition: undefined
}

export function atlasReducer(state?: AtlasState, action?: AnyAction) {
  if (!state) {
    return ATLAS_INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case FETCH_DATA_FROM_SCENE_JSON:
      return reduceFetchDataFromSceneJson(state, action.payload)
    case SUCCESS_DATA_FROM_SCENE_JSON:
      return reduceSuccessDataFromSceneJson(state, action.payload)
    case FAILURE_DATA_FROM_SCENE_JSON:
      return reduceFailureDataFromSceneJson(state, action.payload)
    case MARKET_DATA:
      return reduceMarketData(state, action.payload)
    case REPORTED_SCENES_FOR_MINIMAP:
      return reduceReportedScenesForMinimap(state, action.payload)
    case DISTRICT_DATA:
      return reduceDistrictData(state, action)
  }
  return state
}

function reduceFetchDataFromSceneJson(state: AtlasState, sceneId: string) {
  state.idToScene[sceneId].requestStatus = 'loading'
  return state
}

function reduceFailureDataFromSceneJson(state: AtlasState, sceneId: string) {
  state.idToScene[sceneId].requestStatus = 'fail'
  return state
}

function reduceSuccessDataFromSceneJson(state: AtlasState, landData: ILand) {
  let mapScene: MapSceneData = state.idToScene[landData.sceneId]
  mapScene.requestStatus = 'ok'
  mapScene.sceneJsonData = landData.sceneJsonData

  mapScene.sceneJsonData.scene.parcels.forEach(x => {
    state.tileToScene[x] = mapScene
  })

  state.scenes.add(mapScene)
  return state
}

function reduceDistrictData(state: AtlasState, action: AnyAction) {
  state.hasDistrictData = true
  return state
  // {
  //   ...state,
  //   districtName: {
  //     ...state.districtName,
  //     ...zip(action.payload.data, (t: District) => [t.id, t.name])
  //   }
  // }
}

function reduceReportedScenesForMinimap(
  state: AtlasState,
  payload: { parcels: string[]; reportPosition?: Vector2Component }
) {
  state.lastReportPosition = state.lastReportPosition ?? state.lastReportPosition

  payload.parcels.forEach(x => (state.tileToScene[x].alreadyReported = true))

  return state
  // return {
  //   ...state,
  //   lastReportPosition: action.payload.reportPosition ? action.payload.reportPosition : state.lastReportPosition,
  //   atlasReducer: {
  //     ...state.alreadyReported,
  //     ...action.payload.parcels.reduce((prev: Record<string, boolean>, next: string) => {
  //       prev[next] = true
  //       return prev
  //     }, {})
  //   }
  // }
}

function reduceMarketData(state: AtlasState, marketData: MarketData) {
  state.hasMarketData = true

  Object.keys(marketData.data).forEach(key => {
    let tileToScene = state.tileToScene[key]
    let value = marketData.data[key]

    if (tileToScene) {
      tileToScene.name = getNameFromAtlasState(state, value.x, value.y)
      tileToScene.type = getTypeFromAtlasState(state, value.x, value.y)
      tileToScene.estateId = value.estate_id
      return
    }

    const newScene: MapSceneData = {
      sceneId: '',
      name: getNameFromAtlasState(state, value.x, value.y),
      type: getTypeFromAtlasState(state, value.x, value.y),
      estateId: value.estate_id,
      sceneJsonData: undefined,
      alreadyReported: false,
      requestStatus: undefined
    }

    state.tileToScene[key] = newScene
    state.scenes.add(newScene)
  })

  return state
}
