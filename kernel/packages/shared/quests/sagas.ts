import { userAuthentified } from 'shared/session'
import { getCurrentIdentity } from 'shared/session/selectors'
import { select } from 'redux-saga/effects'
import { QuestsClient } from 'dcl-quests-client'
import { Authenticator } from 'dcl-crypto'

export function* questsSaga() {
  yield userAuthentified()

  const identity = yield select(getCurrentIdentity)

  const questsClient = new QuestsClient({
    baseUrl: 'https://quests-api.decentraland.io',
    authChainProvider: (payload) => Authenticator.signPayload(identity, payload)
  })

  console.log(yield questsClient.getQuests())
}
