import { action } from 'typesafe-actions'
import { Realm } from './types'

export const WEB3_INITIALIZED = 'Web3 initialized'

export const web3initialized = () => action(WEB3_INITIALIZED)
export type Web3Initialized = ReturnType<typeof web3initialized>

export const SET_KATALYST_REALM = 'Set Katalyst realm'
export const setKatalystRealm = (realm: Realm) => action(SET_KATALYST_REALM, realm)
export type SetKatalystRealm = ReturnType<typeof setKatalystRealm>

export const KATALYST_REALM_INITIALIZED = 'Katalyst realm initialized'
export const katalystNodeInitialized = () => action(KATALYST_REALM_INITIALIZED)
export type KatalystNodeInitialized = ReturnType<typeof katalystNodeInitialized>
