import { AnyAction } from 'redux'
import { PARCEL_LOADING_STARTED, RendererState, RENDERER_INITIALIZED } from './types'

const INITIAL_STATE: RendererState = {
  initialized: false,
  parcelLoadingStarted: false
}

export function rendererReducer(state?: RendererState, action?: AnyAction): RendererState {
  if (!state) {
    return INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case RENDERER_INITIALIZED:
      return {
        ...state,
        initialized: true
      }
    case PARCEL_LOADING_STARTED:
      return {
        ...state,
        parcelLoadingStarted: true
      }
    default:
      return state
  }
}
