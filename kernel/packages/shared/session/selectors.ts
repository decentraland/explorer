import { IEthereumProvider, LoginState } from '@dcl/kernel-interface'
import { RootSessionState } from './types'

// TODO use userId
export const getCurrentUserId = (store: RootSessionState) => store.session.identity?.address
export const getCurrentIdentity = (store: RootSessionState) => store.session.identity
export const getCurrentNetwork = (store: RootSessionState) => store.session.network
export const hasWallet = (store: RootSessionState) => store.session.identity?.hasConnectedWeb3
export const getSignUpProfile = (store: RootSessionState) => store.session.signup.profile
export const getSignUpIdentity = (store: RootSessionState) => store.session.signup.identity
export const getIsGuestLogin = (state: RootSessionState): boolean => !!state.session.isGuestLogin
export const getProvider = (state: RootSessionState): IEthereumProvider | undefined => state.session.provider
export const isSignUp = (state: RootSessionState) => state.session.isSignUp
export const isLoginStageCompleted = (state: RootSessionState) => state.session.loginState === LoginState.COMPLETED
