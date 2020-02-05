import { WEB3_INITIALIZED, katalystNodeInitialized, setKatalystRealm } from './actions'
import { call, put, takeEvery } from 'redux-saga/effects'
import { pickKatalystRealm } from './index'
import { Realm } from './types'

export function* daoSaga(): any {
  yield takeEvery(WEB3_INITIALIZED, loadKatalystRealm)
}

function* loadKatalystRealm() {
  const realm: Realm = yield call(pickKatalystRealm)

  yield put(setKatalystRealm(realm))

  yield put(katalystNodeInitialized())
}
