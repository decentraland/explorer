import { action } from 'typesafe-actions'
import { ExplorerIdentity } from './types'
import { ETHEREUM_NETWORK } from '../../config/index'

export const LOGIN = '[Request] Login'
export const login = () => action(LOGIN)
export type Login = ReturnType<typeof login>

export const LOGIN_COMPLETED = '[Success] Login'
export const loginCompleted = (userId: string, identity: ExplorerIdentity, network: ETHEREUM_NETWORK) =>
  action(LOGIN_COMPLETED, { userId, identity, network })
export type LoginCompleted = ReturnType<typeof loginCompleted>

export const LOGOUT = '[Request] Logout'
export const logout = () => action(LOGOUT)
export type Logout = ReturnType<typeof logout>
