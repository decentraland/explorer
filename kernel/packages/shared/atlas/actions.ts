import { action } from 'typesafe-actions'
import {
  DistrictData,
  DISTRICT_DATA,
  FAILURE_DATA_FROM_SCENE_JSON,
  FETCH_DATA_FROM_SCENE_JSON,
  MarketData,
  MARKET_DATA,
  QUERY_DATA_FROM_SCENE_JSON,
  SUCCESS_DATA_FROM_SCENE_JSON,
  REPORT_SCENES_AROUND_PARCEL
} from './types'
import { Vector2Component } from '../../atomicHelpers/landHelpers'
import { ILand } from 'shared/types'

export const querySceneData = (scene: string) => action(QUERY_DATA_FROM_SCENE_JSON, scene)
export type QuerySceneData = ReturnType<typeof querySceneData>

export const fetchDataFromSceneJson = (scene: string) => action(FETCH_DATA_FROM_SCENE_JSON, scene)
export const fetchDataFromSceneJsonSuccess = (scene: string, data: ILand) => action(SUCCESS_DATA_FROM_SCENE_JSON, { sceneId: scene, data: data })
export const fetchDataFromSceneJsonFailure = (scene: string, e: any) => action(FAILURE_DATA_FROM_SCENE_JSON, { sceneId: scene, error: e })
export type FetchDataFromSceneJsonSuccess = ReturnType<typeof fetchDataFromSceneJsonSuccess>

export const districtData = (districts: DistrictData) => action(DISTRICT_DATA, districts)
export const marketData = (data: MarketData) => action(MARKET_DATA, data)
export type MarketDataAction = ReturnType<typeof marketData>

export const REPORTED_SCENES_FOR_MINIMAP = 'Reporting scenes for minimap'
export const reportedScenes = (parcels: string[], reportPosition?: Vector2Component) => action(REPORTED_SCENES_FOR_MINIMAP, { parcels, reportPosition })

export const reportScenesAroundParcel = (parcelCoord: { x: number; y: number }, scenesAround: number) => action(REPORT_SCENES_AROUND_PARCEL, { parcelCoord, scenesAround })
export type ReportScenesAroundParcel = ReturnType<typeof reportScenesAroundParcel>
