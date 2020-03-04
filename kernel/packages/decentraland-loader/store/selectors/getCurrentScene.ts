import { SceneContentServerEntity, SceneInfoState, SceneId } from '../sceneInfo/types'
import { RootState } from '../state'

export function getCurrentSceneEntity(state: RootState): SceneContentServerEntity {
  const position = state.position.targetPosition
  const sceneId = state.sceneInfo.positionToSceneId[position!]
  return buildSceneEntity(state.sceneInfo, sceneId)
}

function buildSceneEntity(state: SceneInfoState, sceneId: SceneId): SceneContentServerEntity {
  return {
    id: sceneId,
    type: 'scene',
    content: state.sceneIdToMappings[sceneId],
    metadata: state.sceneIdToSceneJson[sceneId],
    pointers: state.sceneIdToPositions[sceneId]
  }
}
