import { PlazaNames, RootAtlasState, AtlasState, MapSceneData, MarketEntry } from './types'
import { getSceneTitle } from 'shared/selectors'

export const EMPTY_PARCEL_NAME = 'Empty parcel'

export function getSceneJsonName(state: RootAtlasState, sceneId: string) {
  return getSceneTitle(state.atlas.idToScene[sceneId].sceneJsonData)
}

export function shouldLoadSceneJsonData(state: RootAtlasState, sceneId: string) {
  return state.atlas.idToScene[sceneId] && state.atlas.idToScene[sceneId].requestStatus === undefined
}

export function getType(state: RootAtlasState, x: number, y: number): number {
  const key = `${x},${y}`
  if (!state.atlas.tileToScene[key] || !state.atlas.tileToScene[key].type) {
    return 9
  }
  return state.atlas.tileToScene[key] && state.atlas.tileToScene[key].type
}

export function getMapScene(state: AtlasState, x: number, y: number): MapSceneData {
  return state.tileToScene[`${x},${y}`]
}

function getNameInternal(state: AtlasState, x: number, y: number): string {
  const key = `${x},${y}`

  const sceneJsonName = getSceneTitle(getMapScene(state, x, y).sceneJsonData)

  if (sceneJsonName) {
    return sceneJsonName
  }

  let marketEntry: MarketEntry = state.marketData.data[key]

  if (marketEntry) {
    if (marketEntry.name) {
      return marketEntry.name
    }

    let hasEstate: boolean = marketEntry.estate_id !== undefined
    if (hasEstate && PlazaNames[marketEntry.estate_id]) {
      return PlazaNames[marketEntry.estate_id]
    }
  }

  return EMPTY_PARCEL_NAME
}

export function getNameFromAtlasState(state: AtlasState, x: number, y: number): string {
  let tentativeName = getNameInternal(state, x, y)

  if (tentativeName === 'interactive-text') {
    return EMPTY_PARCEL_NAME
  }

  if (tentativeName.startsWith('Road at')) {
    return 'Road'
  }

  return tentativeName
}

export function getTypeFromAtlasState(state: AtlasState, x: number, y: number): number {
  const key = `${x},${y}`
  return state.tileToScene[key] ? state.tileToScene[key].type : 0
}

export function getName(state: RootAtlasState, x: number, y: number): string {
  return getNameFromAtlasState(state.atlas, x, y)
}
