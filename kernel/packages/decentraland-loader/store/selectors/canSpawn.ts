import { RootState } from '../state'

export function shouldSelectSpawnTarget(state: RootState) {
  if (state.position.isRendering) {
    return false
  }
  if (state.position.hasSelectedSpawnTarget) {
    return false
  }
  if (!state.sceneInfo.positionToSceneId[state.position.targetPosition]) {
    return false
  }
  return true
}
