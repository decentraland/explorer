import { AnyAction } from 'redux'
import { PENDING_SCENES, InformPendingScenes, UPDATE_STATUS_MESSAGE } from './actions'
import {
  FATAL_ERROR,
  ExecutionLifecycleEvent,
  EXPERIENCE_STARTED,
  NOT_STARTED,
  SET_ERROR_TLD,
  SET_LOADING_WAIT_TUTORIAL,
  TELEPORT_TRIGGERED,
  RENDERING_ACTIVATED,
  RENDERING_DEACTIVATED,
  RENDERING_FOREGROUND,
  RENDERING_BACKGROUND
} from './types'

export type LoadingState = {
  status: ExecutionLifecycleEvent
  totalScenes: number
  pendingScenes: number
  message: string
  renderingActivated: boolean
  isForeground: boolean
  initialLoad: boolean
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
      totalScenes: 0,
      pendingScenes: 0,
      message: '',
      renderingActivated: false,
      isForeground: true,
      initialLoad: true,
      error: null,
      tldError: null
    }
  }
  if (!action) {
    return state
  }
  if (action.type === PENDING_SCENES) {
    return {
      ...state,
      pendingScenes: (action as InformPendingScenes).payload.pendingScenes,
      totalScenes: (action as InformPendingScenes).payload.totalScenes
    }
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
  if (action.type === EXPERIENCE_STARTED) {
    return { ...state, status: action.type, initialLoad: false }
  }
  if (action.type === TELEPORT_TRIGGERED) {
    return { ...state, message: action.payload }
  }
  if (action.type === UPDATE_STATUS_MESSAGE) {
    return { ...state, message: action.payload.message }
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
