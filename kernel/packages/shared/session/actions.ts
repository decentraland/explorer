import { action } from 'typesafe-actions'

import { ETHEREUM_NETWORK } from 'config'

import { ExplorerIdentity, SignUpData } from './types'

export const SETUP_WEB3 = '[Request] Initializing web3'
export const setupWeb3 = () => action(SETUP_WEB3)
export type SetupWeb3 = ReturnType<typeof setupWeb3>

export const LOGIN = '[Request] Login'
export const login = () => action(LOGIN)
export type Login = ReturnType<typeof login>

export const LOGIN_GUEST = '[Request] Login Guest'
export const loginGuest = () => action(LOGIN_GUEST)
export type LoginGuest = ReturnType<typeof loginGuest>

export const SIGNUP_FORM = '[SIGN-UP] signup save form'
export const signupForm = (values: SignUpData) => action(SIGNUP_FORM, values)
export type SignUpFormAction = ReturnType<typeof signupForm>

export const SIGNUP_AGREE = '[SIGN-UP] signup agree'
export const signupAgree = () => action(SIGNUP_AGREE, true)
export type SignAgreeAction = ReturnType<typeof signupAgree>

export const SIGNUP = '[Request] Signup'
export const signup = () => action(SIGNUP)
export type Signup = ReturnType<typeof signup>

export const USER_AUTHENTIFIED = '[Success] User authentified'
export const userAuthentified = (userId: string, identity: ExplorerIdentity, network: ETHEREUM_NETWORK) =>
  action(USER_AUTHENTIFIED, { userId, identity, network })
export type UserAuthentified = ReturnType<typeof userAuthentified>

export const LOGIN_COMPLETED = '[Success] Login'
export const loginCompleted = () => action(LOGIN_COMPLETED)
export type LoginCompleted = ReturnType<typeof loginCompleted>

export const LOGOUT = '[Request] Logout'
export const logout = () => action(LOGOUT)
export type Logout = ReturnType<typeof logout>
