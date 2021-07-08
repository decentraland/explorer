import { ETHEREUM_NETWORK, PREVIEW, WORLD_EXPLORER } from 'config'
import { RequestManager } from 'eth-connect'
import { future } from 'fp-future'
import Html from 'shared/Html'
import { checkTldVsWeb3Network, getAppNetwork } from 'shared/web3'
import { IEthereumProvider } from '../../../../anti-corruption-layer/kernel-types'

export const requestManager = new RequestManager((window as any).ethereum ?? null)

export type LoginCompletedResult = { provider: IEthereumProvider; isGuest: boolean }
export const loginCompleted = future<LoginCompletedResult>()

export function login(provider: IEthereumProvider, isGuest: boolean) {
  if (!loginCompleted.isPending) throw new Error('Double login is not enabled')
  loginCompleted.resolve({ provider, isGuest })
}

loginCompleted.then(async ({ provider }) => {
  requestManager.setProvider(provider)

  if (WORLD_EXPLORER && (await checkTldVsWeb3Network())) {
    throw new Error('Network mismatch')
  }

  if (PREVIEW && ETHEREUM_NETWORK.MAINNET === (await getAppNetwork())) {
    Html.showNetworkWarning()
  }
})

export async function isGuest(): Promise<boolean> {
  return (await loginCompleted).isGuest
}

export function isSessionExpired(userData: any) {
  return !userData || !userData.identity || new Date(userData.identity.expiration) < new Date()
}

export async function getUserAccount(returnChecksum: boolean = false): Promise<string | undefined> {
  try {
    const accounts = await requestManager.eth_accounts()

    if (!accounts || accounts.length === 0) {
      return undefined
    }

    return returnChecksum ? accounts[0] : accounts[0].toLowerCase()
  } catch (error) {
    throw new Error(`Could not access eth_accounts: "${error.message}"`)
  }
}
