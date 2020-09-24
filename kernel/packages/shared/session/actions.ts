import { action } from 'typesafe-actions'

import { ETHEREUM_NETWORK } from 'config'

import { AuthError, ExplorerIdentity } from './types'
import { Profile } from '../profiles/types'

export const SETUP_WEB3 = '[Request] Initializing web3'
export const setupWeb3 = () => action(SETUP_WEB3)
export type SetupWeb3 = ReturnType<typeof setupWeb3>

export const LOGIN = '[Request] Login'
export const login = (provider: string, email: string) =>
  action(LOGIN, { provider, values: new Map([['email', email]]) })
export type LoginAction = ReturnType<typeof login>

export const SIGNUP_ACTIVE = '[SIGN-UP] signup active'
export const signUpActive = (active: boolean) => action(SIGNUP_ACTIVE, active)
export type SignUpActiveAction = ReturnType<typeof signUpActive>

export const SIGNUP_FORM = '[SIGN-UP] signup save form'
export const signupForm = (unverifiedName: string, email: string) => action(SIGNUP_FORM, { unverifiedName, email })
export type SignUpFormAction = ReturnType<typeof signupForm>

export const SIGNUP_AGREE = '[SIGN-UP] signup agree'
export const signupAgree = () => action(SIGNUP_AGREE, true)
export type SignAgreeAction = ReturnType<typeof signupAgree>

export const SIGNUP_SET_PROFILE = '[SIGN-UP] signup set profile'
export const signupSetProfile = (profile: Partial<Profile>) => action(SIGNUP_SET_PROFILE, profile)
export type SignSetProfileAction = ReturnType<typeof signupSetProfile>

export const SIGNUP = '[Request] Signup'
export const signup = (provider: string) => action(SIGNUP, { provider })
export type SignupAction = ReturnType<typeof signup>

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

export const AUTH_ERROR = '[connector] signIn/signUp error'
export const authError = (error: AuthError, errorMsg: string | null = null) => action(AUTH_ERROR, { error, errorMsg })
export type AuthErrorAction = ReturnType<typeof authError>

export const AUTH_CLEAR_ERROR = '[Request] connector clear error'
export const authClearError = () => action(AUTH_CLEAR_ERROR)
