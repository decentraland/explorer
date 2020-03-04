import { generateSightMap, sceneSightDelta } from '../sceneSight/sceneSight'
import { RootState } from '../state'

export function generateSightMapFromState(state: RootState) {
  return generateSightMap(state.sightInfo.inSight, state.sceneInfo.positionToSceneId)
}

export function generateSightDeltaFromState(state: RootState) {
  const currentSight = state.sceneSight
  const newSight = generateSightMapFromState(state)
  return sceneSightDelta(currentSight, newSight)
}
