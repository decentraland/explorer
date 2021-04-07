import { AnyAction } from 'redux'

import { RendererState, RENDERER_INITIALIZED } from './types'

const INITIAL_STATE: RendererState = {
  initialized: false
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
    default:
      return state
  }
}
