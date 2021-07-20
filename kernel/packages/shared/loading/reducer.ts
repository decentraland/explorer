import { AnyAction } from 'redux'
import { SCENE_FAIL, SCENE_LOAD, SCENE_START, UPDATE_STATUS_MESSAGE } from './actions'
import {
  FATAL_ERROR,
  ExecutionLifecycleEvent,
  ExecutionLifecycleEventsList,
  EXPERIENCE_STARTED,
  loadingTips,
  NOT_STARTED,
  ROTATE_HELP_TEXT,
  SET_ERROR_TLD,
  SET_LOADING_SCREEN,
  SET_LOADING_WAIT_TUTORIAL,
  SUBSYSTEMS_EVENTS,
  TELEPORT_TRIGGERED,
  RENDERING_ACTIVATED,
  RENDERING_DEACTIVATED,
  RENDERING_FOREGROUND,
  RENDERING_BACKGROUND
} from './types'

export type LoadingState = {
  status: ExecutionLifecycleEvent
  helpText: number
  pendingScenes: number
  message: string
  subsystemsLoad: number
  loadPercentage: number
  renderingActivated: boolean
  isForeground: boolean
  initialLoad: boolean
  showLoadingScreen: boolean
  waitingTutorial?: boolean
  error: string | null
  tldError: {
    tld: string
    web3Net: string
    tldNet: string
  } | null
}

export type RootLoadingState = {
  loading: LoadingState
}

export function loadingReducer(state?: LoadingState, action?: AnyAction): LoadingState {
  if (!state) {
    return {
      status: NOT_STARTED,
      helpText: 0,
      pendingScenes: 0,
      message: '',
      renderingActivated: false,
      isForeground: true,
      loadPercentage: 0,
      subsystemsLoad: 0,
      initialLoad: true,
      showLoadingScreen: false,
      error: null,
      tldError: null
    }
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
  if (action.type === RENDERING_ACTIVATED) {
    return { ...state, renderingActivated: true }
  }
  if (action.type === RENDERING_DEACTIVATED) {
    return { ...state, renderingActivated: false }
  }
  if (action.type === RENDERING_FOREGROUND) {
    return { ...state, isForeground: true }
  }
  if (action.type === RENDERING_BACKGROUND) {
    return { ...state, isForeground: false }
  }
  if (ExecutionLifecycleEventsList.includes(action.type)) {
    const newState = { ...state, status: action.type }
    if (SUBSYSTEMS_EVENTS.includes(action.type)) {
      newState.subsystemsLoad = state.subsystemsLoad + 100 / SUBSYSTEMS_EVENTS.length
    }
    if (EXPERIENCE_STARTED === action.type) {
      newState.initialLoad = false
    }
    return newState
  }
  if (action.type === TELEPORT_TRIGGERED) {
    return { ...state, helpText: 0, message: action.payload }
  }
  if (action.type === ROTATE_HELP_TEXT) {
    const newValue = state.helpText + 1
    return { ...state, helpText: newValue >= loadingTips.length ? 0 : newValue }
  }
  if (action.type === UPDATE_STATUS_MESSAGE) {
    return { ...state, message: action.payload.message, loadPercentage: action.payload.loadPercentage }
  }
  if (action.type === SET_LOADING_SCREEN) {
    return { ...state, showLoadingScreen: action.payload.showLoadingScreen }
  }
  if (action.type === SET_LOADING_WAIT_TUTORIAL) {
    return { ...state, waitingTutorial: action.payload.waiting }
  }
  if (action.type === FATAL_ERROR) {
    return { ...state, error: action.payload.type }
  }
  if (action.type === SET_ERROR_TLD) {
    return { ...state, error: 'networkmismatch', tldError: action.payload }
  }
  return state
}
