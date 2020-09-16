import {
  WEB3_INITIALIZED,
  catalystRealmInitialized,
  initCatalystRealm,
  setCatalystCandidates,
  setAddedCatalystCandidates,
  setContentWhitelist,
  INIT_CATALYST_REALM,
  SET_CATALYST_REALM,
  InitCatalystRealm,
  SetCatalystRealm,
  SET_CATALYST_CANDIDATES,
  SET_ADDED_CATALYST_CANDIDATES,
  SetCatalystCandidates,
  SetAddedCatalystCandidates,
  CATALYST_REALM_INITIALIZED,
  catalystRealmsScanSuccess,
  catalystRealmsScanRequested
} from './actions'
import { call, put, takeEvery, select, fork, take } from 'redux-saga/effects'
import { WORLD_EXPLORER, REALM, getDefaultTLD, PIN_CATALYST } from 'config'
import { waitForMetaConfigurationInitialization } from '../meta/sagas'
import { Candidate, Realm, ServerConnectionStatus } from './types'
import {
  fecthCatalystRealms,
  fetchCatalystStatuses,
  pickCatalystRealm,
  getRealmFromString,
  ping,
  commsStatusUrl
} from '.'
import { getAddedServers, getContentWhitelist } from 'shared/meta/selectors'
import { getAllCatalystCandidates, isRealmInitialized } from './selectors'
import { saveToLocalStorage, getFromLocalStorage } from '../../atomicHelpers/localStorage'
import defaultLogger from '../logger'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { CATALYST_COULD_NOT_LOAD } from 'shared/loading/types'

const CACHE_KEY = 'realm'
const CATALYST_CANDIDATES_KEY = CACHE_KEY + '-' + SET_CATALYST_CANDIDATES
const CACHE_TLD_KEY = 'tld'

export function* daoSaga(): any {
  yield call(loadDefaultCatalystRealms)
  yield takeEvery(WEB3_INITIALIZED, loadCatalystRealms)

  yield takeEvery([INIT_CATALYST_REALM, SET_CATALYST_REALM], cacheCatalystRealm)
  yield takeEvery([SET_CATALYST_CANDIDATES, SET_ADDED_CATALYST_CANDIDATES], cacheCatalystCandidates)
}

function* loadDefaultCatalystRealms() {
  yield call(waitForMetaConfigurationInitialization)

  const tld = getDefaultTLD().toLowerCase() !== 'org' ? 'zone' : 'org'
  const realm: Realm = {
    domain: `https://peer.decentraland.${tld}`,
    catalystName: 'fenrir',
    layer: 'blue',
    lighthouseVersion: '0.2'
  }
  const whiteList = {
    domain: `https://peer.decentraland.${tld}`,
    catalystName: 'fenrir',
    elapsed: 544,
    status: 0,
    layer: {
      name: 'blue',
      usersCount: 0,
      maxUsers: 100,
      usersParcels: []
    },
    score: -50,
    lighthouseVersion: '0.2'
  }
  yield put(setContentWhitelist([whiteList]))
  yield put(initCatalystRealm(realm))
}

/**
 * This method will try to load the candidates as well as the selected realm.
 *
 * The strategy to select the realm in terms of priority is:
 * 1- Realm configured in the URL and cached candidate for that realm (uses cache, forks async candidadte initialization)
 * 2- Realm configured in the URL but no corresponding cached candidate (implies sync candidate initialization)
 * 3- Last cached realm (uses cache, forks async candidadte initialization)
 * 4- Best pick from candidate scan (implies sync candidate initialization)
 */
function* loadCatalystRealms() {
  yield call(waitForMetaConfigurationInitialization)

  if (WORLD_EXPLORER) {
    const cachedRealm: Realm | undefined = getFromLocalStorage(CACHE_KEY)
    const cachedTld: string | undefined = getFromLocalStorage(CACHE_TLD_KEY)

    let realm: Realm | undefined

    // check for cached realms if any
    if (cachedRealm && cachedTld === getDefaultTLD() && (!PIN_CATALYST || cachedRealm.domain === PIN_CATALYST)) {
      const cachedCandidates: Candidate[] = getFromLocalStorage(CATALYST_CANDIDATES_KEY) ?? []

      let configuredRealm: Realm
      if (REALM) {
        // if a realm is configured, then try to initialize it from cached candidates
        configuredRealm = yield call(getConfiguredRealm, cachedCandidates)
      } else {
        // in case there are no cached candidates or the realm was not configured in the URL -> use last cached realm
        configuredRealm = cachedRealm
      }

      if (configuredRealm && (yield checkValidRealm(configuredRealm))) {
        realm = configuredRealm

        yield fork(initializeCatalystCandidates)
      }
    }

    // if no realm was selected, then do the whole initialization dance
    if (!realm) {
      try {
        yield call(initializeCatalystCandidates)
      } catch (e) {
        ReportFatalError(CATALYST_COULD_NOT_LOAD)
        throw e
      }

      realm = yield call(selectRealm)
    }

    saveToLocalStorage(CACHE_TLD_KEY, getDefaultTLD())

    yield put(initCatalystRealm(realm!))
  } else {
    yield put(setCatalystCandidates([]))
    yield put(setAddedCatalystCandidates([]))
    yield put(setContentWhitelist([]))
    yield put(
      initCatalystRealm({
        domain: window.location.origin,
        catalystName: 'localhost',
        layer: 'stub',
        lighthouseVersion: '0.1'
      })
    )
  }

  yield put(catalystRealmInitialized())

  defaultLogger.info(`Using Catalyst configuration: `, yield select((state) => state.dao))
}

export function* selectRealm() {
  const allCandidates: Candidate[] = yield select(getAllCatalystCandidates)

  let realm = yield call(getConfiguredRealm, allCandidates)
  if (!realm) {
    realm = yield call(pickCatalystRealm, allCandidates)
  }
  return realm
}

function getConfiguredRealm(candidates: Candidate[]) {
  if (REALM) {
    return getRealmFromString(REALM, candidates)
  }
}

function* initializeCatalystCandidates() {
  yield put(catalystRealmsScanRequested())
  const candidates: Candidate[] = yield call(fecthCatalystRealms)

  yield put(setCatalystCandidates(candidates))

  const added: string[] = yield select(getAddedServers)
  const addedCandidates: Candidate[] = yield call(
    fetchCatalystStatuses,
    added.map((url) => ({ domain: url }))
  )

  yield put(setAddedCatalystCandidates(addedCandidates))

  const allCandidates: Candidate[] = yield select(getAllCatalystCandidates)

  const whitelist: string[] = yield select(getContentWhitelist)
  let whitelistedCandidates = allCandidates.filter((candidate) => whitelist.includes(candidate.domain))
  if (whitelistedCandidates.length === 0) {
    // if intersection is empty (no whitelisted or not in our candidate set) => whitelist all candidates
    whitelistedCandidates = allCandidates
  }

  yield put(setContentWhitelist(whitelistedCandidates))
  yield put(catalystRealmsScanSuccess())
}

async function checkValidRealm(realm: Realm) {
  return (
    realm.domain &&
    realm.catalystName &&
    realm.layer &&
    (await ping(commsStatusUrl(realm.domain))).status === ServerConnectionStatus.OK
  )
}

function* cacheCatalystRealm(action: InitCatalystRealm | SetCatalystRealm) {
  saveToLocalStorage(CACHE_KEY, action.payload)
}

function* cacheCatalystCandidates(action: SetCatalystCandidates | SetAddedCatalystCandidates) {
  const allCandidates = yield select(getAllCatalystCandidates)

  saveToLocalStorage(CATALYST_CANDIDATES_KEY, allCandidates)
}

export function* ensureRealmInitialized() {
  while (!(yield select(isRealmInitialized))) {
    yield take(CATALYST_REALM_INITIALIZED)
  }
}
