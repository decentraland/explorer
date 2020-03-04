import { SceneId, StringCoordinate } from '../sceneInfo/types'
import { FlexibleSightMap, SceneSightState, SightMap } from './types'

export function increaseSightWithMap(state: SceneSightState, map: Record<SceneId, number>, keys?: string[]) {
  if (!keys) {
    keys = Object.keys(map)
  }
  const newState = { ...state }
  for (let key of keys) {
    if (!newState[key]) {
      newState[key] = 0
    }
    newState[key] += map[key]
    if (newState[key] === 0) {
      delete newState[key]
    }
  }
  return newState
}

export function generateSightMap(inSight: StringCoordinate[], map: SightMap) {
  const result: Record<SceneId, number> = {}
  for (let parcel of inSight) {
    if (map[parcel]) {
      result[map[parcel]] = (result[map[parcel]] || 0) + 1
    }
  }
  return result
}

export function sceneSightDelta(
  from: FlexibleSightMap,
  to: FlexibleSightMap,
  keysFrom: SceneId[] = Object.keys(from),
  keysTo: SceneId[] = Object.keys(to)
) {
  const result: {
    lostSight: SceneId[]
    newInSight: SceneId[]
    currentSight: typeof to
  } = {
    lostSight: [],
    newInSight: [],
    currentSight: to
  }
  for (let key of keysFrom) {
    if (!to[key]) {
      result.lostSight.push(key)
    }
  }
  for (let key of keysTo) {
    if (!from[key]) {
      result.newInSight.push(key)
    }
  }
  return result
}
