import { AnyAction } from 'redux'
import { SET_CATALYST_REALM, INIT_CATALYST_REALM, SET_CATALYST_CANDIDATES } from './actions'
import { DaoState, Candidate, Realm } from './types'
import {
  FETCH_PROFILE_SERVICE,
  FETCH_CONTENT_SERVICE,
  UPDATE_CONTENT_SERVICE,
  COMMS_SERVICE,
  REALM as REALM_QUERY
} from '../../config/index'

function getConfiguredRealm(candidates: Candidate[]) {
  if (REALM_QUERY) {
    const parts = REALM_QUERY.split('-')
    if (parts.length == 2) {
      return realmFor(parts[0], parts[1], candidates)
    }
  }
}

function realmFor(name: string, layer: string, candidates: Candidate[]): Realm | undefined {
  const candidate = candidates.find(it => it.catalystName === name && it.layer.name === layer)
  return candidate
    ? { catalystName: candidate.catalystName, domain: candidate.domain, layer: candidate.layer.name }
    : undefined
}

export function daoReducer(state?: DaoState, action?: AnyAction): DaoState {
  if (!state) {
    return {
      initialized: false,
      profileServer: '',
      fetchContentServer: '',
      updateContentServer: '',
      commsServer: '',
      layer: '',
      realm: undefined,
      candidates: []
    }
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SET_CATALYST_CANDIDATES:
      return {
        ...state,
        candidates: action.payload
      }
    case INIT_CATALYST_REALM: {
      const configuredRealm = getConfiguredRealm(state.candidates)
      const realm = configuredRealm ? configuredRealm : action.payload
      return {
        ...state,
        initialized: true,
        ...realmProperties(realm)
      }
    }
    case SET_CATALYST_REALM:
      return {
        ...state,
        ...realmProperties(action.payload, !!action.payload.configOverride)
      }
    default:
      return state
  }
}
function realmProperties(realm: Realm, configOverride: boolean = true) {
  const domain = realm.domain
  return {
    profileServer: FETCH_PROFILE_SERVICE && configOverride ? FETCH_PROFILE_SERVICE : domain + '/lambdas/profile',
    fetchContentServer: FETCH_CONTENT_SERVICE && configOverride ? FETCH_CONTENT_SERVICE : domain + '/lambdas/contentv2',
    updateContentServer: UPDATE_CONTENT_SERVICE && configOverride ? UPDATE_CONTENT_SERVICE : domain + '/content',
    commsServer: COMMS_SERVICE && configOverride ? COMMS_SERVICE : domain + '/comms',
    realm
  }
}
