import { AnyAction } from "redux"
import { KernelAccountState, KernelResult } from "../../../anti-corruption-layer/kernel-types"
import { SET_KERNEL_ACCOUNT_STATE, SET_KERNEL_LOADED } from "./actions"
import { KernelState, SessionState, RendererState, ErrorState, FeatureFlags } from "./redux"

export function kernelReducer(state: KernelState | undefined, action: AnyAction): KernelState {
  if (action.type == SET_KERNEL_LOADED) {
    return { ...state, ready: true, kernel: action.payload as KernelResult }
  }
  return (
    state || {
      ready: false,
      kernel: null,
    }
  )
}

export function sessionReducer(state: SessionState | undefined, action: AnyAction): SessionState {
  if (action.type === SET_KERNEL_ACCOUNT_STATE) {
    return { ...state, kernelState: action.payload as KernelAccountState }
  }
  return state || { kernelState: null }
}

export function rendererReducer(state: RendererState | undefined, action: any): RendererState {
  return {
    ready: false,
    version: "latest",
  }
}

export function featureFlagsReducer(state: FeatureFlags | undefined, action: any): FeatureFlags {
  return {
    sessionId: "",
  }
}

export function errorReducer(state: ErrorState | undefined, action: any): ErrorState | null {
  return state || null
}
