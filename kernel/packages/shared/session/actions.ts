import { action } from 'typesafe-actions'

import { ETHEREUM_NETWORK } from 'config'

import { ExplorerIdentity } from './types'

export const INIT_SESSION = '[Session] initializing'
export const initSession = () => action(INIT_SESSION)
export type InitSession = ReturnType<typeof initSession>

export const AUTHENTICATE = '[Authenticate]'
export const authenticate = (provider: string) => action(AUTHENTICATE, { provider })
export type AuthenticateAction = ReturnType<typeof authenticate>

export const SIGNUP = '[SIGNUP]'
export const signUp = () => action(SIGNUP)

export const LOGIN = '[Request] Login'
export const login = (provider: string) => action(LOGIN, { provider })
export type LoginAction = ReturnType<typeof login>

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

export const UPDATE_TOS = 'UPDATE_TOS'
export const updateTOS = (agreed: boolean) => action(UPDATE_TOS, agreed)

export const SIGNUP_FORM = '[SIGNUP_FORM]'
export const signupForm = (name: string, email: string) => action(SIGNUP_FORM, { name, email })
export type SignUpFormAction = ReturnType<typeof signupForm>

export const SIGNUP_STAGE = '[SIGNUP_STAGE]'
export const changeSignUpStage = (stage: string) => action(SIGNUP_STAGE, { stage })
export type ChangeSignUpStageAction = ReturnType<typeof changeSignUpStage>

export const CHANGE_LOGIN_STAGE = '[LOGIN_STAGE] change login stage'
export const changeLoginStage = (stage: string) => action(CHANGE_LOGIN_STAGE, { stage })

export const TOGGLE_WALLET_PROMPT = '[WALLET_PROMPT] show wallet prompt'
export const toggleWalletPrompt = (show: boolean) => action(TOGGLE_WALLET_PROMPT, { show })
