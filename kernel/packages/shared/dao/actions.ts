import { ETHEREUM_NETWORK } from 'config'
import { action } from 'typesafe-actions'
import { Realm, Candidate, CommsStatus } from './types'

export const WEB3_INITIALIZED = 'Web3 initialized'

export const UPDATE_CATALYST_REALM = 'Update Catalyst Realm'

export const SET_CATALYST_CANDIDATES = 'Set Catalyst Candidates'
export const setCatalystCandidates = (candidates: Candidate[]) => action(SET_CATALYST_CANDIDATES, candidates)
export type SetCatalystCandidates = ReturnType<typeof setCatalystCandidates>

export const SELECT_NETWORK = '[DAO] Select network'
export const selectNetwork = (network: ETHEREUM_NETWORK) => action(SELECT_NETWORK, network)
export type SelectNetworkAction = ReturnType<typeof selectNetwork>

export const SET_ADDED_CATALYST_CANDIDATES = 'Set Added Catalyst Candidates'
export const setAddedCatalystCandidates = (candidates: Candidate[]) => action(SET_ADDED_CATALYST_CANDIDATES, candidates)
export type SetAddedCatalystCandidates = ReturnType<typeof setAddedCatalystCandidates>

export const SET_CONTENT_WHITELIST = 'Set Content Whitelist'
export const setContentWhitelist = (candidates: Candidate[]) => action(SET_CONTENT_WHITELIST, candidates)
export type SetContentWhitelist = ReturnType<typeof setContentWhitelist>

export const INIT_CATALYST_REALM = 'Init Catalyst realm'
export const initCatalystRealm = (realm: Realm) => action(INIT_CATALYST_REALM, realm)
export type InitCatalystRealm = ReturnType<typeof initCatalystRealm>

export const SET_CATALYST_REALM = 'Set Catalyst realm'
export const setCatalystRealm = (realm: Realm) => action(SET_CATALYST_REALM, realm)
export type SetCatalystRealm = ReturnType<typeof setCatalystRealm>

export const SET_CATALYST_REALM_COMMS_STATUS = 'Set Catalyst Realm Comms Status'
export const setCatalystRealmCommsStatus = (status: CommsStatus) => action(SET_CATALYST_REALM_COMMS_STATUS, status)
export type SetCatalystRealmCommsStatus = ReturnType<typeof setCatalystRealmCommsStatus>

export const MARK_CATALYST_REALM_FULL = 'Mark Catalyst Realm Full'
export const markCatalystRealmFull = (realm: Realm) => action(MARK_CATALYST_REALM_FULL, realm)
export type MarkCatalystRealmFull = ReturnType<typeof markCatalystRealmFull>

export const MARK_CATALYST_REALM_CONNECTION_ERROR = 'Mark Catalyst Realm Connection Error'
export const markCatalystRealmConnectionError = (realm: Realm) => action(MARK_CATALYST_REALM_CONNECTION_ERROR, realm)
export type MarkCatalystRealmConnectionError = ReturnType<typeof markCatalystRealmConnectionError>

export const CATALYST_REALMS_SCAN_REQUESTED = '[Request] Catalyst Realms scan'
export const catalystRealmsScanRequested = () => action(CATALYST_REALMS_SCAN_REQUESTED)
export type CatalystRealmsScanRequested = ReturnType<typeof catalystRealmsScanRequested>

export const CATALYST_REALMS_SCAN_SUCCESS = '[Success] Catalyst Realms scan'
export const catalystRealmsScanSuccess = () => action(CATALYST_REALMS_SCAN_SUCCESS)
export type CatalystRealmsScanSuccess = ReturnType<typeof catalystRealmsScanSuccess>
