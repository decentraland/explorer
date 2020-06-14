import { DEBUG, EDITOR, ENGINE_DEBUG_PANEL, SCENE_DEBUG_PANEL, SHOW_FPS_COUNTER, tutorialEnabled } from 'config'
import { globalDCL } from 'shared/globalDCL'
import { unityClientLoaded } from 'shared/loading/types'
import { defaultLogger } from 'shared/logger'
import { browserInterface } from './browserInterface'
import { initializeDecentralandUI } from './initializeDecentralandUI'
import { setupPosition } from './position/setupPosition'
import { setupPointerLock } from './setupPointerLock'
import { unityInterface } from './unityInterface'

/**
 *
 * Common initialization logic for the unity engine
 *
 * @param _gameInstance Unity game instance
 */

export async function initializeEngine(_gameInstance: any) {
  globalDCL.lowLevelInterface = _gameInstance

  setupPosition()

  setupPointerLock()

  globalDCL.globalStore.dispatch(unityClientLoaded())
  globalDCL.rendererInterface.SetLoadingScreenVisible(true)

  globalDCL.rendererInterface.DeactivateRendering()

  if (DEBUG) {
    globalDCL.rendererInterface.SetDebug()
  }

  if (SCENE_DEBUG_PANEL) {
    globalDCL.rendererInterface.SetSceneDebugPanel()
  }

  if (SHOW_FPS_COUNTER) {
    globalDCL.rendererInterface.ShowFPSPanel()
  }

  if (ENGINE_DEBUG_PANEL) {
    globalDCL.rendererInterface.SetEngineDebugPanel()
  }

  if (tutorialEnabled()) {
    globalDCL.rendererInterface.SetTutorialEnabled()
  }

  if (!EDITOR) {
    await initializeDecentralandUI()
  }

  return {
    unityInterface: unityInterface,
    onMessage(type: string, message: any) {
      if (type in browserInterface) {
        // tslint:disable-next-line:semicolon
        ;(browserInterface as any)[type](message)
      } else {
        defaultLogger.info(`Unknown message (did you forget to add ${type} to unity-interface/dcl.ts?)`, message)
      }
    }
  }
}
