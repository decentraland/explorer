import { RequestManager } from 'eth-connect'
import { future } from 'fp-future'
import { defaultLogger } from 'shared/logger'
import { Account } from 'web3x/account'
import { Eth } from 'web3x/eth'
import { Web3Connector } from './Web3Connector'
import { ProviderType } from './ProviderType'
import { WORLD_EXPLORER } from '../../config'

let web3Connector: Web3Connector
export const providerFuture = future()
export const requestManager = new RequestManager(null)

export const loginCompleted = future<void>()
;(window as any).loginCompleted = loginCompleted

let providerRequested = false

type LoginData = { successful: boolean; provider: any; localIdentity?: Account }

export function createEth(provider: any = null): Eth {
  return web3Connector.createEth(provider)!
}

export function createWeb3Connector(): Web3Connector {
  if (!web3Connector) {
    web3Connector = new Web3Connector()
  }
  return web3Connector
}

export async function requestWeb3Provider(type: ProviderType) {
  try {
    const provider = await web3Connector.connect(type)
    requestManager.setProvider(provider)
    providerFuture.resolve({
      successful: true,
      provider: provider
    })
    return provider
  } catch (e) {
    defaultLogger.log('Could not get a wallet connection', e)
    requestManager.setProvider(null)
  }
  return null
}

export async function awaitWeb3Approval(): Promise<void> {
  if (!providerRequested) {
    providerRequested = true
    if (!WORLD_EXPLORER) {
      // otherwise, login element not found (preview, builder)
      providerFuture.resolve({
        successful: false,
        provider: Web3Connector.createWebSocketProvider(),
        localIdentity: Account.create()
      })
    }
  }
  providerFuture.then((result: LoginData) => requestManager.setProvider(result.provider)).catch(defaultLogger.error)
  return providerFuture
}

export function isSessionExpired(userData: any) {
  return !userData || !userData.identity || new Date(userData.identity.expiration) < new Date()
}

export async function getUserAccount(): Promise<string | undefined> {
  try {
    const eth = createEth()
    const accounts = await eth.getAccounts()

    if (!accounts || accounts.length === 0) {
      return undefined
    }

    return accounts[0].toJSON().toLocaleLowerCase()
  } catch (error) {
    throw new Error(`Could not access eth_accounts: "${error.message}"`)
  }
}
