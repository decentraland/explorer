import { PREVIEW } from '../../config'
import { RootLoadingState } from './reducer'

export const isInitialLoading = (state: RootLoadingState) => state.loading.initialLoad
export const isWaitingTutorial = (state: RootLoadingState) => state.loading.waitingTutorial

export function hasPendingScenes(state: RootLoadingState) {
  return state.loading.pendingScenes !== 0
}

export function isLoadingScreenVisible(state: RootLoadingState) {
  // if it is the initial load
  if (state.loading.initialLoad) {
    // if it has pending scenes in the initial load, then the loading
    // screen should be visible
    if (hasPendingScenes(state)) {
      return true
    }

    if (state.loading.totalScenes == 0 && !PREVIEW) {
      // this may happen if we are loading for the first time and this saga
      // gets executed _before_ the initial load of scenes
      return true
    }
  }

  // if the camera is offline, it definitely means we are loading.
  // This logic should be handled by Unity
  // Teleporting is also handled by this function. Since rendering is
  // deactivated on Position.unsettled events
  return !state.loading.renderingActivated
}

export function isRendererVisible(state: RootLoadingState) {
  return state.loading.renderingActivated || isLoadingScreenVisible(state)
}
