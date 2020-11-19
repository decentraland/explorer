import { exposeMethod, registerAPI } from 'decentraland-rpc/lib/host'
import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { ExposableAPI } from './ExposableAPI'
import { defaultLogger } from '../logger'
import { DEBUG } from '../../config'

type SceneState = any

@registerAPI('SceneStateStorageController')
export class SceneStateStorageController extends ExposableAPI {

  @exposeMethod
  async storeState(sceneId: string, sceneState: SceneState): Promise<void> {
    if (DEBUG) {
      saveToLocalStorage(`scene-state-${sceneId}`, sceneState)
    } else {
      defaultLogger.error('Content server storage not yet supported')
      saveToLocalStorage(`scene-state-${sceneId}`, sceneState)
    }
  }

  @exposeMethod
  async getStoredState(sceneId: string): Promise<SceneState> {
    if (DEBUG) {
      const sceneState = getFromLocalStorage(`scene-state-${sceneId}`)
      if (!sceneState) {
        defaultLogger.warn(`Couldn't find a stored scene state for scene ${sceneId}`)
        return { entities: [] }
      }
      return sceneState
    } else {
      defaultLogger.error('Content server storage not yet supported')
      const sceneState = getFromLocalStorage(`scene-state-${sceneId}`)
      return sceneState ?? { entities: [] }
    }
  }
}
