import { Observable } from 'mz-observable'
import type {
  KernelError,
  KernelLoadingProgress,
  KernelTrackingEvent,
  KernelAccountState,
  KernelSignUpEvent
} from '../../../anti-corruption-layer/kernel-types'

export const errorObservable = new Observable<KernelError>()
export const loadingProgressObservable = new Observable<KernelLoadingProgress>()
export const trackingEventObservable = new Observable<KernelTrackingEvent>()
export const accountStateObservable = new Observable<KernelAccountState>()
export const signUpObservable = new Observable<KernelSignUpEvent>()
