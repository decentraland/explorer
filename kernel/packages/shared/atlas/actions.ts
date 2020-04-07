import { ILand } from 'shared/types'
import { action } from 'typesafe-actions'
import { Vector2Component } from '../../atomicHelpers/landHelpers'
import {
  DistrictData,
  DISTRICT_DATA,
  FAILURE_DATA_FROM_SCENE_JSON,
  MarketData,
  MARKET_DATA,
  QUERY_DATA_FROM_SCENE_JSON,
  REPORT_SCENES_AROUND_PARCEL,
  SUCCESS_DATA_FROM_SCENE_JSON
} from './types'

export const querySceneData = (scene: string) => action(QUERY_DATA_FROM_SCENE_JSON, scene)
export type QuerySceneData = ReturnType<typeof querySceneData>

export const fetchDataFromSceneJsonSuccess = (sceneId: string, data: ILand) =>
  action(SUCCESS_DATA_FROM_SCENE_JSON, { sceneId, data })
export const fetchDataFromSceneJsonFailure = (sceneId: string, error: any) =>
  action(FAILURE_DATA_FROM_SCENE_JSON, { sceneId, error })
export type FetchDataFromSceneJsonSuccess = ReturnType<typeof fetchDataFromSceneJsonSuccess>
export type FetchDataFromSceneJsonFailure = ReturnType<typeof fetchDataFromSceneJsonFailure>

export const districtData = (districts: DistrictData) => action(DISTRICT_DATA, districts)
export const marketData = (data: MarketData) => action(MARKET_DATA, data)
export type MarketDataAction = ReturnType<typeof marketData>

export const REPORTED_SCENES_FOR_MINIMAP = 'Reporting scenes for minimap'
export const reportedScenes = (parcels: string[], reportPosition?: Vector2Component) =>
  action(REPORTED_SCENES_FOR_MINIMAP, { parcels, reportPosition })

export const reportScenesAroundParcel = (parcelCoord: { x: number; y: number }, rectSizeAround: number) =>
  action(REPORT_SCENES_AROUND_PARCEL, { parcelCoord, scenesAround: rectSizeAround })
export type ReportScenesAroundParcel = ReturnType<typeof reportScenesAroundParcel>
