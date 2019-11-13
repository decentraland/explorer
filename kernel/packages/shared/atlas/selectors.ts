import { PlazaNames, RootAtlasState, AtlasState } from './types'

export function getSceneJsonName(state: RootAtlasState, sceneName: string) {
  return state.atlas.sceneNames[sceneName]
}

export function shouldLoadSceneJsonName(state: RootAtlasState, sceneName: string) {
  return state.atlas.requestStatus[sceneName] === undefined
}

export function getType(state: RootAtlasState, x: number, y: number): number {
  const key = `${x},${y}`
  return (state.atlas.marketName[key] && state.atlas.marketName[key].type) || 9
}

export function getName(state: RootAtlasState, x: number, y: number): string {
  const key = `${x},${y}`
  const name = state.atlas.sceneNames[key]
    ? state.atlas.sceneNames[key]
    : state.atlas.marketName[key] && state.atlas.marketName[key].name
    ? state.atlas.marketName[key].name
    : state.atlas.marketName[key] &&
      state.atlas.marketName[key].estate_id &&
      PlazaNames[state.atlas.marketName[key].estate_id]
    ? PlazaNames[state.atlas.marketName[key].estate_id]
    : 'Empty parcel'
  if (name === 'interactive-text') {
    return 'Empty parcel'
  }
  if (name.startsWith('Road at')) {
    return 'Road'
  }
  return name
}

export function getNameFromAtlasState(state: AtlasState, x: number, y: number): string {
  const key = `${x},${y}`
  const name = state.sceneNames[key]
    ? state.sceneNames[key]
    : state.marketName[key] && state.marketName[key].name
    ? state.marketName[key].name
    : state.marketName[key] && state.marketName[key].estate_id && PlazaNames[state.marketName[key].estate_id]
    ? PlazaNames[state.marketName[key].estate_id]
    : 'Empty parcel'
  if (name === 'interactive-text') {
    return 'Empty parcel'
  }
  if (name.startsWith('Road at')) {
    return 'Road'
  }
  return name
}
