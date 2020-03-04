import { splitCoordinate } from '../utils/splitCoordinate'
import {
  ValidSceneState,
  SceneContentServerEntity,
  StringCoordinate,
  SceneId,
  CLOSENESS_RADIUS_FOR_GARBAGE_COLLECTING
} from './types'

// Incorporate land info

export function incorporateLandInfo(state: ValidSceneState, landInfo: SceneContentServerEntity): ValidSceneState {
  if (state.sceneIdToMappings[landInfo.id]) {
    return state
  }
  if (!landInfo.metadata || !landInfo.metadata.scene || !landInfo.metadata.scene.scene) {
    throw new Error('Invalid info:' + JSON.stringify(landInfo, null, 2) + 'has no scene')
  }
  return {
    positionToSceneId: {
      ...state.positionToSceneId,
      ...landInfo.pointers.reduce((cumm: Record<StringCoordinate, SceneId>, item: StringCoordinate) => {
        cumm[item] = landInfo.id
        return cumm
      }, {})
    },
    sceneIdToPositions: { ...state.sceneIdToPositions, [landInfo.id]: landInfo.pointers },
    sceneIdToMappings: { ...state.sceneIdToMappings, [landInfo.id]: landInfo.content },
    sceneIdToSceneJson: { ...state.sceneIdToSceneJson, [landInfo.id]: landInfo.metadata }
  }
}

// Garbage collection

export function garbageCollect(state: ValidSceneState, relativeTo: StringCoordinate) {
  const farAwayScenes = getFarAwayScenes(state, relativeTo)
  const result = { ...state }
  for (let sceneId of farAwayScenes) {
    delete state.sceneIdToMappings[sceneId]
    delete state.sceneIdToSceneJson[sceneId]
    delete state.sceneIdToPositions[sceneId]
  }
  const flatFarAwayPositions = getPositionsFromSceneIdSet(state, farAwayScenes)
  for (let position of flatFarAwayPositions) {
    delete state.positionToSceneId[position]
  }
  return result
}

function getFarAwayScenes(state: ValidSceneState, relativeTo: StringCoordinate) {
  const [x, y] = splitCoordinate(relativeTo)
  function closeToPosition(position: StringCoordinate) {
    const [w, z] = splitCoordinate(position)
    return Math.abs(w - x) + Math.abs(y - z) < CLOSENESS_RADIUS_FOR_GARBAGE_COLLECTING
  }
  return Object.keys(state.sceneIdToPositions).filter(sceneId => {
    const positions = state.sceneIdToPositions[sceneId]
    return positions.some(closeToPosition)
  })
}

function getPositionsFromSceneIdSet(state: ValidSceneState, sceneIds: SceneId[]) {
  const result = []
  for (let sceneId of sceneIds) {
    result.push(...state.sceneIdToPositions[sceneId])
  }
  return result
}
