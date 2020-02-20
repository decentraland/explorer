import { WEB3_INITIALIZED, catalystRealmInitialized, initCatalystRealm, setCatalystCandidates } from './actions'
import { call, put, takeEvery, take, select } from 'redux-saga/effects'
import { pickCatalystRealm, fecthCatalystRealms, fetchCatalystStatuses } from './index'
import { Realm, Candidate } from './types'
import { META_CONFIGURATION_INITIALIZED } from '../meta/actions'
import { getAddedServers } from '../meta/selectors'

export function* daoSaga(): any {
  yield takeEvery(WEB3_INITIALIZED, loadCatalystRealms)
}

function* loadCatalystRealms() {
  yield take(META_CONFIGURATION_INITIALIZED)

  const candidates: Candidate[] = yield call(fecthCatalystRealms)

  yield put(setCatalystCandidates(candidates))

  const added: string[] = yield select(getAddedServers)
  const addedCandidates: Candidate[] = yield call(fetchCatalystStatuses, added.map(url => ({ domain: url })))

  const realm: Realm = yield call(pickCatalystRealm, candidates.concat(addedCandidates))

  yield put(initCatalystRealm(realm))

  yield put(catalystRealmInitialized())
}
