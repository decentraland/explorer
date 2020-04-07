import { Vector2Component } from 'atomicHelpers/landHelpers'
import { SceneJsonData } from 'shared/types'

export const UPDATE_MINIMAP_SCENE_NAME = 'Update tile name'
export const QUERY_DATA_FROM_SCENE_JSON = '[Query] Fetch data from scene.json'
export const SUCCESS_DATA_FROM_SCENE_JSON = '[Success] Fetch data from scene.json'
export const FAILURE_DATA_FROM_SCENE_JSON = '[Failure] Fetch data from scene.json'
export const DISTRICT_DATA = '[Info] District data downloaded'
export const MARKET_DATA = '[Info] Market data downloaded'
export const REPORT_SCENES_AROUND_PARCEL = 'Report scenes around parcel'

export type AtlasState = {
  hasMarketData: boolean
  hasDistrictData: boolean

  tileToScene: Record<string, MapSceneData> // '0,0' -> sceneId. Useful for mapping tile market data to actual scenes.
  idToScene: Record<string, MapSceneData> // sceneId -> MapScene
  lastReportPosition?: Vector2Component
}

export type MapSceneData = {
  sceneId: string
  name: string
  type: number
  estateId?: number
  sceneJsonData?: SceneJsonData
  alreadyReported: boolean
  requestStatus: undefined | 'loading' | 'ok' | 'fail'
}

export type RootAtlasState = {
  atlas: AtlasState
}

export type DistrictData = {
  ok: boolean
  data: District[]
}

export type District = {
  id: string
  name: string
}

export type MarketData = {
  ok: boolean
  data: Record<string, MarketEntry>
}

export type MarketEntry = {
  x: number
  y: number
  name: string
  estate_id?: number
  type: number
}

export const PlazaNames: { [key: number]: string } = {
  1134: 'Vegas Plaza',
  1092: 'Forest Plaza',
  1094: 'CyberPunk Plaza',
  1132: 'Soho Plaza',
  1096: 'Medieval Plaza',
  1130: 'Gamer Plaza',
  1127: 'SciFi Plaza',
  1112: 'Asian Plaza',
  1164: 'Genesis Plaza',
  1825: 'Roads',
  1813: 'Roads',
  1815: 'Roads',
  1827: 'Roads',
  1824: 'Roads',
  1820: 'Roads',
  1924: 'Roads',
  1925: 'Roads',
  1830: 'Roads',
  1831: 'Roads',
  1832: 'Roads'
}
