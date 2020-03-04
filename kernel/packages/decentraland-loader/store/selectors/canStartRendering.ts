import { STARTED } from '../sceneSight/sceneStatus'
import { RootState } from '../state'

const DEBUG_START_RENDER = false

export function canStartRendering(state: RootState) {
  if (state.position.isRendering) {
    if (DEBUG_START_RENDER) console.log('Already rendering')
    return false
  }
  if (!state.position.hasSelectedSpawnTarget) {
    if (DEBUG_START_RENDER) console.log('No spawn')
    return false
  }
  if (state.download.queued.length > 0 || Object.keys(state.download.pendingDownloads).length > 0) {
    if (DEBUG_START_RENDER) console.log('No queued')
    return false
  }
  const sceneId = state.sceneInfo.positionToSceneId[(state.position as any).targetPosition!]
  if (!state.sceneInfo.sceneIdToSceneJson[sceneId] || state.sceneState[sceneId] !== STARTED) {
    if (DEBUG_START_RENDER) console.log('Not started', state)
    return false
  }
  const scenesInSight = Object.keys(state.sceneSight)
  if (DEBUG_START_RENDER) console.log(`Scenes in sight: ${scenesInSight.length}`)
  for (let scene of scenesInSight) {
    if (!state.sceneState[scene] || state.sceneState[scene] !== STARTED) {
      if (DEBUG_START_RENDER) console.log(`${scene} has not started yet`, state)
      return false
    }
  }
  if (DEBUG_START_RENDER) console.log('spawning', state)
  return true
}
