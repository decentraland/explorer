import { Vector2Component } from 'atomicHelpers/landHelpers'
import { AnyAction } from 'redux'
import { ILand } from 'shared/types'
import { REPORTED_SCENES_FOR_MINIMAP, FetchDataFromSceneJsonSuccess, QuerySceneData, FetchDataFromSceneJsonFailure } from './actions'
import { getSceneNameFromAtlasState, getSceneNameWithMarketAndAtlas, postProcessSceneName } from './selectors'
// @ts-ignore
import defaultLogger from '../logger'
import {
  AtlasState,
  DISTRICT_DATA,
  FAILURE_DATA_FROM_SCENE_JSON,
  MapSceneData,
  MarketData,
  MARKET_DATA,
  SUCCESS_DATA_FROM_SCENE_JSON,
  QUERY_DATA_FROM_SCENE_JSON
} from './types'

const ATLAS_INITIAL_STATE: AtlasState = {
  hasMarketData: false,
  hasDistrictData: false,
  tileToScene: {}, // '0,0' -> sceneId. Useful for mapping tile market data to actual scenes.
  idToScene: {}, // sceneId -> MapScene
  lastReportPosition: undefined
}

const MAP_SCENE_DATA_INITIAL_STATE: MapSceneData = {
  sceneId: '',
  name: '',
  type: 0,
  estateId: 0,
  sceneJsonData: undefined,
  alreadyReported: false,
  requestStatus: 'loading'
}

export function atlasReducer(state?: AtlasState, action?: AnyAction) {
  if (!state) {
    return ATLAS_INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case QUERY_DATA_FROM_SCENE_JSON:
      return reduceFetchDataFromSceneJson(state, (action as QuerySceneData).payload)
    case SUCCESS_DATA_FROM_SCENE_JSON:
      return reduceSuccessDataFromSceneJson(state, (action as FetchDataFromSceneJsonSuccess).payload.data)
    case FAILURE_DATA_FROM_SCENE_JSON:
      return reduceFailureDataFromSceneJson(state, (action as FetchDataFromSceneJsonFailure).payload.sceneId)
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
  if (!state.idToScene[sceneId]) {
    state.idToScene[sceneId] = { ...MAP_SCENE_DATA_INITIAL_STATE }
  }

  state.idToScene[sceneId].requestStatus = 'loading'
  return state
}

function reduceFailureDataFromSceneJson(state: AtlasState, sceneId: string) {
  if (!state.idToScene[sceneId]) {
    state.idToScene[sceneId] = { ...MAP_SCENE_DATA_INITIAL_STATE }
  }

  state.idToScene[sceneId].requestStatus = 'fail'
  return state
}

function reduceSuccessDataFromSceneJson(state: AtlasState, landData: ILand) {
  const tileToScene = { ...state.tileToScene }
  const idToScene = { ...state.idToScene }

  let mapScene: MapSceneData = { ...state.idToScene[landData.sceneId] }

  // NOTE(Brian): this code is for the case in which market data comes first (most likely always)
  //             in that case we find the tileToScene data and update the mapScene with the
  //             relevant market values. If we don't do this we will get an inconsistent state.
  landData.sceneJsonData.scene.parcels.forEach(x => {
    if (state.tileToScene[x]) {
      mapScene = {
        ...mapScene,
        name: state.tileToScene[x].name,
        type: state.tileToScene[x].type,
        estateId: state.tileToScene[x].estateId
      }
    }
  })

  mapScene.requestStatus = 'ok'
  mapScene.sceneJsonData = landData.sceneJsonData

  const name = getSceneNameFromAtlasState(state, mapScene.sceneJsonData) ?? mapScene.name
  mapScene.name = postProcessSceneName(name)

  mapScene.sceneJsonData.scene.parcels.forEach(x => {
    tileToScene[x] = mapScene
  })

  idToScene[landData.sceneId] = mapScene

  return { ...state, tileToScene, idToScene }
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
}

function reduceMarketData(state: AtlasState, marketData: MarketData) {
  const tileToScene = { ...state.tileToScene }

  Object.keys(marketData.data).forEach(key => {
    const existingScene = state.tileToScene[key]
    const value = marketData.data[key]

    const sceneName = postProcessSceneName(getSceneNameWithMarketAndAtlas(marketData, state, value.x, value.y))

    let newScene: MapSceneData

    if (existingScene) {
      newScene = {
        ...existingScene,
        name: sceneName,
        type: value.type,
        estateId: value.estate_id
      }
    } else {
      newScene = {
        sceneId: '',
        name: sceneName,
        type: value.type,
        estateId: value.estate_id,
        sceneJsonData: undefined,
        alreadyReported: false,
        requestStatus: undefined
      }
    }

    tileToScene[key] = newScene
  })

  return { ...state, hasMarketData: true }
}
