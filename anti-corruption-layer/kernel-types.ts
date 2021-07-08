// This interface is the anti corruption layer between kernel and website

import type { Observable } from "../kernel/node_modules/mz-observable"
import type { AuthIdentity } from "../kernel/node_modules/dcl-crypto/dist"

export type IEthereumProvider = { sendAsync: any } | { request: any }

export interface KernelTrackingEvent {
  eventName: string
  eventData: Record<string, any>
}

export interface KernelError {
  error: Error
  code?: string
  level?: "critical" | "fatal"
  extra?: Record<string, any>
}

export interface KernelLoadingProgress {
  progress: number
  status?: number
}

export enum LoginStage {
  LOADING = "loading",
  SIGN_IN = "signIn",
  SIGN_UP = "signUp",
  CONNECT_ADVICE = "connect_advice",
  SIGN_ADVICE = "sign_advice",
  COMPLETED = "completed",
}

export type DecentralandIdentity = AuthIdentity & {
  address: string // contains the lowercased address that will be used for the userId
  rawAddress: string // contains the real ethereum address of the current user
  provider?: any
  hasConnectedWeb3: boolean
}

export interface KernelAccountState {
  loginStatus: LoginStage
  network?: string
  identity?: DecentralandIdentity
  signing: boolean
  hasProvider: boolean
}

export interface KernelSignUpEvent {
  email: string
}

export type KernelOptions = {
  container: any
  kernelOptions: {
    version: string
    baseUrl: string
  }
  rendererOptions: {
    version: string
    baseUrl: string
  }
}

export type KernelResult = {
  signUpObservable: Observable<KernelSignUpEvent>
  accountStateObservable: Observable<KernelAccountState>
  loadingProgressObservable: Observable<KernelLoadingProgress>
  errorObservable: Observable<KernelError>
  trackingEventObservable: Observable<KernelTrackingEvent>
  authenticate(provider: IEthereumProvider, isGuest: boolean): void
  version: string
}

export interface IDecentralandKernel {
  initKernel(options: KernelOptions): Promise<KernelResult>
}
