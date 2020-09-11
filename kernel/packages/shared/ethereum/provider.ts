import { WebSocketProvider, RequestManager } from 'eth-connect'
import { future } from 'fp-future'

import { ethereumConfigurations, ETHEREUM_NETWORK } from 'config'
import { defaultLogger } from 'shared/logger'
import { Account } from 'web3x/account'
import { getUserProfile, removeUserProfile } from 'shared/comms/peers'
import { getTLD } from '../../config'
import { Eth } from 'web3x/eth'
import Web3Connector from './Web3Connector'

declare var window: Window & {
  ethereum: any
  web3: any
}
export const web3Connector: Web3Connector = new Web3Connector()

export function createEth(provider: any = null): Eth {
  return web3Connector.createEth(provider)!
}

export const providerFuture = future()
export const requestManager = new RequestManager(null)

export const loginCompleted = future<void>()
;(window as any).loginCompleted = loginCompleted

let providerRequested = false

type LoginData = { successful: boolean; provider: any; localIdentity?: Account }

export async function awaitWeb3Approval(): Promise<void> {
  if (!providerRequested) {
    providerRequested = true

    const userData = getUserProfile()
    if (isSessionExpired(userData)) {
      removeUserProfile()
    }

    const element = document.getElementById('eth-login')
    if (element) {
      element.style.display = 'block'

      const button = document.getElementById('eth-login-confirm-button')
      button!.onclick = async () => {
        try {
          const provider = await web3Connector.connect()

          providerFuture.resolve({
            successful: true,
            provider: provider
          })
        } catch (e) {
          console.log('Could not get a wallet connection', e)
          requestManager.setProvider(null)
          providerFuture.reject(e)
        }
      }
    } else {
      // otherwise, login element not found (preview, builder)
      providerFuture.resolve({
        successful: false,
        provider: createProvider(),
        localIdentity: Account.create()
      })
    }
  }

  providerFuture.then((result: LoginData) => requestManager.setProvider(result.provider)).catch(defaultLogger.error)

  return providerFuture
}

function createProvider() {
  const network = getTLD() === 'zone' ? ETHEREUM_NETWORK.ROPSTEN : ETHEREUM_NETWORK.MAINNET
  return new WebSocketProvider(ethereumConfigurations[network].wss)
}

export function isSessionExpired(userData: any) {
  return !userData || !userData.identity || new Date(userData.identity.expiration) < new Date()
}

export async function getUserAccount(): Promise<string | undefined> {
  try {
    const eth = createEth()!
    const accounts = await eth.getAccounts()

    if (!accounts || accounts.length === 0) {
      return undefined
    }

    return accounts[0].toJSON().toLocaleLowerCase()
  } catch (error) {
    throw new Error(`Could not access eth_accounts: "${error.message}"`)
  }
}
