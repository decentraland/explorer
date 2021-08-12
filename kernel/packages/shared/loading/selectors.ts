import { LoginState } from '@dcl/kernel-interface'
import { RootRendererState } from 'shared/renderer/types'
import { RootSessionState } from 'shared/session/types'
import { RootState } from 'shared/store/rootTypes'
import { RootLoadingState } from './reducer'

export const isInitialLoading = (state: RootLoadingState) => state.loading.initialLoad
export const isWaitingTutorial = (state: RootLoadingState) => state.loading.waitingTutorial

export function hasPendingScenes(state: RootLoadingState) {
  return state.loading.pendingScenes !== 0
}

export function isLoadingScreenVisible(state: RootLoadingState & RootSessionState & RootRendererState) {
  const { session, renderer } = state

  // in the case of signup, we show the avatars editor instead of the loading screen
  // that is so, to enable the user to customize the avatar while loading the world
  if (!session.identity && session.isSignUp && session.loginState === LoginState.WAITING_PROFILE) {
    return false
  }

  // if parcel loading is not yet started, the loading screen should be visible
  if (!renderer.parcelLoadingStarted) {
    return true
  }

  // if it is the initial load
  if (state.loading.initialLoad) {
    // if it has pending scenes in the initial load, then the loading
    // screen should be visible
    if (hasPendingScenes(state)) {
      return true
    }

    if (state.loading.totalScenes === 0) {
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

// the strategy with this function is to fail fast with "false" and then
// cascade until find a "true"
export function isRendererVisible(state: RootState) {
  // of course, if the renderer is not initialized, it is not visible
  if (!state.renderer.initialized) {
    return false
  }

  // some login stages requires the renderer to be turned off
  const { loginState } = state.session
  if (
    loginState === LoginState.SIGNATURE_FAILED ||
    loginState === LoginState.SIGNATURE_PENDING ||
    loginState === LoginState.WAITING_PROVIDER
  ) {
    return false
  }

  return state.loading.renderingActivated || isLoadingScreenVisible(state)
}
