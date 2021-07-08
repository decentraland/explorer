import { combineReducers, createStore } from "redux"
import { KernelAccountState, KernelResult } from "../../../anti-corruption-layer/kernel-types"
import { kernelReducer, sessionReducer, rendererReducer, errorReducer, featureFlagsReducer } from "./reducers"

export type KernelState = {
  ready: boolean
  kernel: KernelResult | null
}

export type RendererState = {
  ready: boolean
  version: string
}

export type FeatureFlags = {
  sessionId: string
}

export type SessionState = {
  kernelState: KernelAccountState | null
}

export type ErrorState = {
  type: ErrorType
  details: string
}

export enum ErrorType {
  FATAL = "fatal",
  COMMS = "comms",
  NEW_LOGIN = "newlogin",
  NOT_MOBILE = "nomobile",
  NOT_INVITED = "notinvited",
  NOT_SUPPORTED = "notsupported",
  NET_MISMATCH = "networkmismatch",
  AVATAR_ERROR = "avatarerror",
}

export type StoreType = {
  kernel: KernelState
  renderer: RendererState
  session: SessionState
  featureFlags: FeatureFlags
  error: ErrorState | null
}

const reducers = combineReducers<StoreType>({
  kernel: kernelReducer,
  session: sessionReducer,
  renderer: rendererReducer,
  featureFlags: featureFlagsReducer,
  error: errorReducer,
})

export const store = createStore(reducers)
