import { action } from "typesafe-actions"
import {
  KernelAccountState,
  KernelError,
  KernelLoadingProgress,
  KernelResult,
} from "../../../anti-corruption-layer/kernel-types"

export const SET_KERNEL_ACCOUNT_STATE = "Set kernel account state"
export const SET_KERNEL_ERROR = "Set kernel error"
export const SET_KERNEL_LOADED = "Set kernel loaded"
export const SET_RENDERER_LOADING = "Set renderer loading"

export const setKernelAccountState = (accountState: KernelAccountState) =>
  action(SET_KERNEL_ACCOUNT_STATE, accountState)
export const setKernelError = (error: KernelError) => action(SET_KERNEL_ERROR, error)
export const setKernelLoaded = (kernel: KernelResult) => action(SET_KERNEL_LOADED, kernel)
export const setRendererLoading = (progressEvent: KernelLoadingProgress) => action(SET_RENDERER_LOADING, progressEvent)
