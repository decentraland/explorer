import { AnyAction } from 'redux'
import { SCENE_FAIL, SCENE_LOAD, SCENE_START } from './actions'
import {
  ExecutionLifecycleEvent,
  ExecutionLifecycleEventsList,
  helpTexts,
  NOT_STARTED,
  ROTATE_HELP_TEXT,
  TELEPORT_TRIGGERED
} from './types'

export type LoadingState = {
  status: ExecutionLifecycleEvent
  helpText: number
  pendingScenes: number
}
export const DEBUG_LOADING = false

export function loadingReducer(state?: LoadingState, action?: AnyAction) {
  if (DEBUG_LOADING) {
    // tslint:disable:no-console
    console.log(state && state.pendingScenes, action)
  }

  if (!state) {
    return { status: NOT_STARTED, helpText: 0, pendingScenes: 0 }
  }

  if (!action) {
    return state
  }

  if (action.type === SCENE_LOAD) {
    return { ...state, pendingScenes: state.pendingScenes + 1 }
  }
  if (action.type === SCENE_FAIL) {
    return { ...state, pendingScenes: state.pendingScenes - 1 }
  }
  if (action.type === SCENE_START) {
    return { ...state, pendingScenes: state.pendingScenes - 1 }
  }
  if (ExecutionLifecycleEventsList.includes(action.type)) {
    return { ...state, status: action.type }
  }
  if (action.type === TELEPORT_TRIGGERED) {
    return { ...state, pendingScenes: 0, helpText: action.payload }
  }
  if (action.type === ROTATE_HELP_TEXT) {
    const newValue = state.helpText + 1
    return { ...state, helpText: newValue >= helpTexts.length ? 0 : newValue }
  }
  return state
}
