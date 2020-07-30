import { put, takeEvery, select, call } from 'redux-saga/effects'

import { STATIC_WORLD } from 'config'

import { establishingComms } from 'shared/loading/types'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { getCurrentIdentity } from 'shared/session/selectors'
import { setWorldContext } from 'shared/protocol/actions'
import { ensureRealmInitialized } from 'shared/dao/sagas'

import { connect } from '.'

export function* commsSaga() {
  yield takeEvery(USER_AUTHENTIFIED, establishCommunications)
}

function* establishCommunications() {
  if (STATIC_WORLD) {
    return
  }

  yield call(ensureRealmInitialized)

  const identity = yield select(getCurrentIdentity)

  yield put(establishingComms())
  const context = yield connect(identity.address)
  if (context !== undefined) {
    yield put(setWorldContext(context))
  }
}
