import { StringCoordinate } from '../sceneInfo/types'
import { worldToStringCoordinate, xyToStringCoordinate } from '../utils/worldToGrid'

type Vector3 = {
  x: number
  y: number
  z: number
}
type Vector2 = {
  x: number
  y: number
}

export type PositionState = {
  previousPosition?: StringCoordinate
  targetPosition: StringCoordinate
  currentPosition?: StringCoordinate

  hasSelectedSpawnTarget: boolean
  spawnTarget?: {
    position: Vector3
    cameraTarget: Vector3
  }

  isTeleporting: boolean
  isRendering: boolean
}

export function initialPositionState(targetPosition: StringCoordinate): PositionState {
  return {
    targetPosition,
    hasSelectedSpawnTarget: false,
    isTeleporting: false,
    isRendering: false
  }
}

export function updateStateOnTeleport(state: PositionState, target: StringCoordinate): PositionState {
  return {
    ...state,
    previousPosition: state.currentPosition,
    targetPosition: target,
    currentPosition: target,
    hasSelectedSpawnTarget: false,
    isTeleporting: true,
    isRendering: false
  }
}

export function updateStateOnSpawn(state: PositionState, position: Vector3, cameraTarget: Vector3): PositionState {
  return {
    ...state,
    currentPosition: worldToStringCoordinate(position),
    targetPosition: worldToStringCoordinate(position),
    spawnTarget: {
      cameraTarget,
      position
    },
    hasSelectedSpawnTarget: true
  }
}

export function updateStartRendering(state: PositionState): PositionState {
  return {
    ...state,
    isTeleporting: false,
    isRendering: true
  }
}

export function updateStateOnUserMovement(state: PositionState, userPosition: Vector2): PositionState {
  const position = xyToStringCoordinate(userPosition)

  return {
    ...state,
    currentPosition: position,
    targetPosition: position,
    previousPosition: state.currentPosition
  }
}
