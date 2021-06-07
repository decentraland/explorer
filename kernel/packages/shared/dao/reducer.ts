import { AnyAction } from 'redux'
import {
  SET_CATALYST_REALM,
  INIT_CATALYST_REALM,
  SET_CATALYST_CANDIDATES,
  SET_CATALYST_REALM_COMMS_STATUS,
  MARK_CATALYST_REALM_FULL,
  SET_ADDED_CATALYST_CANDIDATES,
  SET_CONTENT_WHITELIST,
  MARK_CATALYST_REALM_CONNECTION_ERROR
} from './actions'
import { DaoState, Candidate, Realm, ServerConnectionStatus } from './types'
import {
  FETCH_CONTENT_SERVICE,
  UPDATE_CONTENT_SERVICE,
  COMMS_SERVICE,
  RESIZE_SERVICE,
  PIN_CATALYST,
  HOTSCENES_SERVICE,
  POI_SERVICE
} from 'config'

export function daoReducer(state?: DaoState, action?: AnyAction): DaoState {
  if (!state) {
    return {
      initialized: false,
      candidatesFetched: false,
      fetchContentServer: '',
      catalystServer: '',
      updateContentServer: '',
      commsServer: '',
      resizeService: '',
      hotScenesService: '',
      exploreRealmsService: '',
      poiService: '',
      realm: undefined,
      candidates: [],
      addedCandidates: [],
      contentWhitelist: [],
      commsStatus: { status: 'initial', connectedPeers: 0 }
    }
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SET_CATALYST_CANDIDATES:
      return {
        ...state,
        candidatesFetched: true,
        candidates: action.payload
      }
    case SET_ADDED_CATALYST_CANDIDATES:
      return {
        ...state,
        addedCandidates: action.payload
      }
    case SET_CONTENT_WHITELIST:
      return {
        ...state,
        contentWhitelist: action.payload
      }
    case INIT_CATALYST_REALM: {
      return {
        ...state,
        initialized: true,
        ...ensureProfileDao(
          ensureContentWhitelist(realmProperties(action.payload), state.contentWhitelist),
          state.candidates
        )
      }
    }
    case SET_CATALYST_REALM:
      return {
        ...state,
        ...ensureProfileDao(
          ensureContentWhitelist(
            realmProperties(action.payload, !!action.payload.configOverride),
            state.contentWhitelist
          ),
          state.candidates
        )
      }
    case SET_CATALYST_REALM_COMMS_STATUS:
      return {
        ...state,
        commsStatus: action.payload ? action.payload : { status: 'initial', connectedPeers: 0 }
      }
    case MARK_CATALYST_REALM_FULL:
      return {
        ...state,
        candidates: state.candidates.map((it) => {
          if (it && it.catalystName === action.payload.catalystName && it.layer.name === action.payload.layer) {
            return { ...it, layer: { ...it.layer, usersCount: it.layer.maxUsers } }
          } else {
            return it
          }
        })
      }
    case MARK_CATALYST_REALM_CONNECTION_ERROR:
      return {
        ...state,
        candidates: state.candidates.map((it) => {
          if (it && it.catalystName === action.payload.catalystName) {
            return {
              ...it,
              status: ServerConnectionStatus.UNREACHABLE,
              elapsed: Number.MAX_SAFE_INTEGER
            }
          } else {
            return it
          }
        })
      }
    default:
      return state
  }
}
function realmProperties(realm: Realm, configOverride: boolean = true): Partial<DaoState> {
  const domain = realm.domain
  return {
    fetchContentServer: FETCH_CONTENT_SERVICE && configOverride ? FETCH_CONTENT_SERVICE : domain + '/lambdas/contentv2',
    catalystServer: domain,
    updateContentServer: UPDATE_CONTENT_SERVICE && configOverride ? UPDATE_CONTENT_SERVICE : domain + '/content',
    commsServer: COMMS_SERVICE && configOverride ? COMMS_SERVICE : domain + '/comms',
    resizeService: RESIZE_SERVICE && configOverride ? RESIZE_SERVICE : domain + '/lambdas/images',
    hotScenesService: HOTSCENES_SERVICE && configOverride ? HOTSCENES_SERVICE : domain + '/lambdas/explore/hot-scenes',
    poiService: POI_SERVICE && configOverride ? POI_SERVICE : domain + '/lambdas/contracts/pois',
    exploreRealmsService: domain + '/lambdas/explore/realms',
    realm
  }
}

function ensureContentWhitelist(state: Partial<DaoState>, contentWhitelist: Candidate[]): Partial<DaoState> {
  // if a catalyst is pinned => avoid any override
  if (PIN_CATALYST) {
    return state
  }

  // if current realm is in whitelist => return current state
  if (state.realm && contentWhitelist.some((candidate) => candidate.domain === state.realm!.domain)) {
    return state
  }

  if (contentWhitelist.length === 0) {
    return state
  }

  // otherwise => override fetch content server to optimize performance
  const { domain } = contentWhitelist[0]
  return {
    ...state,
    fetchContentServer: FETCH_CONTENT_SERVICE ? FETCH_CONTENT_SERVICE : domain + '/lambdas/contentv2'
  }
}

function ensureProfileDao(state: Partial<DaoState>, daoCandidates: Candidate[]) {
  // if a catalyst is pinned => avoid any override
  if (PIN_CATALYST) {
    return state
  }

  // if current realm is in dao => return current state
  if (state.realm && daoCandidates.some((candidate) => candidate.domain === state.realm!.domain)) {
    return state
  }

  if (daoCandidates.length === 0) {
    return state
  }

  // else if fetch content server is in dao => override fetch & update profile server to use that same one
  let domain: string

  const fetchContentDomain = getContentDomain(state)
  if (daoCandidates.some((candidate) => candidate.domain === fetchContentDomain)) {
    domain = fetchContentDomain
  } else {
    // otherwise => override fetch & update profile server to maintain consistency
    domain = daoCandidates[0].domain
  }

  return {
    ...state,
    updateContentServer: UPDATE_CONTENT_SERVICE ? UPDATE_CONTENT_SERVICE : domain + '/content'
  }
}

function getContentDomain(state: Partial<DaoState>) {
  if (!state.fetchContentServer) {
    return ''
  }

  const service = state.fetchContentServer
  return service.substring(0, service.length - '/lambdas/contentv2'.length)
}
