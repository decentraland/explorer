import { SceneId } from '../sceneInfo/types'
import { UNLOADED } from '../sceneSight/sceneStatus'
import { RootState } from '../state'

export function getSceneStatus(state: RootState, sceneId: SceneId) {
  return state.sceneState[sceneId] || UNLOADED
}

export function getAllScenes(state: RootState) {
  return state.sceneState
}
