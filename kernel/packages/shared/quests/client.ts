import { getServerConfigurations } from '../../config'
import { Authenticator } from 'dcl-crypto'
import { ClientResponse, QuestsClient } from 'dcl-quests-client'
import { onLoginCompleted } from 'shared/session/sagas'
import { store } from 'shared/store/isolatedStore'
import { getSelectedNetwork } from 'shared/dao/selectors'

export async function questsClient() {
  const { identity } = await onLoginCompleted()
  const net = getSelectedNetwork(store.getState())
  const servers = getServerConfigurations(net)
  return new QuestsClient({
    baseUrl: servers.questsUrl,
    // tslint:disable-next-line:no-unnecessary-type-assertion There seems to be a bug with tslint here
    authChainProvider: (payload) => Authenticator.signPayload(identity!, payload)
  })
}

export async function questsRequest<T>(
  request: (client: QuestsClient) => Promise<ClientResponse<T>>
): Promise<ClientResponse<T>> {
  try {
    const client = await questsClient()
    return await request(client)
  } catch (e) {
    return { ok: false, status: 0, body: { status: 'unknown error', message: e.message } }
  }
}
