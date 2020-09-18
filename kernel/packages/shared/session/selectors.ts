import { RootSessionState } from './types'

// TODO use userId
export const isSignUpActive = (store: RootSessionState) => store.session.signup.active
export const getSignUpData = (store: RootSessionState) => store.session.signup
export const getCurrentUserId = (store: RootSessionState) => store.session.identity?.address
export const getCurrentIdentity = (store: RootSessionState) => store.session.identity
export const getCurrentNetwork = (store: RootSessionState) => store.session.network
export const hasWallet = (store: RootSessionState) => store.session.identity?.hasConnectedWeb3
