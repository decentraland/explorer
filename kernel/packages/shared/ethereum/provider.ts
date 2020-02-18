import { WebSocketProvider, RequestManager } from 'eth-connect'
import { future } from 'fp-future'

import { ethereumConfigurations, ETHEREUM_NETWORK } from 'config'
import { defaultLogger } from 'shared/logger'
import { Account } from 'web3x/account'
import { getUserProfile } from 'shared/comms/peers'
import { getTLD } from '../../config/index'
import { getUserAccount } from './EthereumService'
import { removeUserProfile } from '../comms/peers'

declare var window: Window & {
  ethereum: any
  web3: any
}

export const providerFuture = future()
export const requestManager = new RequestManager(null)

let providerRequested = false

export async function awaitWeb3Approval(): Promise<void> {
  if (!providerRequested) {
    providerRequested = true
    // Modern dapp browsers...
    if (window['ethereum']) {
      await removeSessionIfNotValid()

      // TODO - look for user id matching account - moliva - 18/02/2020
      let userData = getUserProfile()

      if (!isSessionExpired(userData)) {
        providerFuture.resolve({ successful: true, provider: window.ethereum })
      } else {
        window['ethereum'].autoRefreshOnNetworkChange = false

        const element = document.getElementById('eth-login')
        if (element) {
          element.style.display = 'block'
          const button = document.getElementById('eth-login-confirm-button')

          let response = future()

          button!.onclick = async () => {
            let result
            try {
              // Request account access if needed
              await window.ethereum.enable()

              result = { successful: true, provider: window.ethereum }
            } catch (error) {
              // User denied account access...
              result = {
                successful: false,
                provider: createProvider()
              }
            }
            response.resolve(result)
          }

          let result
          while (true) {
            result = await response

            element.style.display = 'none'

            const button = document.getElementById('eth-relogin-confirm-button')

            response = future()

            button!.onclick = async () => {
              let result
              try {
                // Request account access if needed
                await window.ethereum.enable()

                result = { successful: true, provider: window.ethereum }
              } catch (error) {
                // User denied account access, need to retry...
                result = { successful: false }
              }
              response.resolve(result)
            }

            if (result.successful) {
              break
            } else {
              showEthConnectAdvice(true)
            }
          }
          showEthConnectAdvice(false)
          providerFuture.resolve(result)
        }
      }
    } else if (window.web3 && window.web3.currentProvider) {
      await removeSessionIfNotValid()

      // legacy providers (don't need for confirmation)
      providerFuture.resolve({ successful: true, provider: window.web3.currentProvider })
    } else {
      // otherwise, create a local identity
      providerFuture.resolve({
        successful: false,
        provider: createProvider(),
        localIdentity: Account.create()
      })
    }
  }

  providerFuture.then(result => requestManager.setProvider(result.provider)).catch(defaultLogger.error)

  return providerFuture
}

/**
 * Remove local session if persisted account does not match with one or ephemeral key is expired
 */
async function removeSessionIfNotValid() {
  const account = await getUserAccount()

  // TODO - look for user id matching account - moliva - 18/02/2020
  let userData = getUserProfile()

  if ((userData && userData.userId !== account) || isSessionExpired(userData)) {
    removeUserProfile()
  }
}

function createProvider() {
  const network = getTLD() === 'zone' ? ETHEREUM_NETWORK.ROPSTEN : ETHEREUM_NETWORK.MAINNET
  return new WebSocketProvider(ethereumConfigurations[network].wss)
}

function showEthConnectAdvice(show: boolean) {
  const element = document.getElementById('eth-connect-advice')
  if (element) {
    element.style.display = show ? 'block' : 'none'
  }
}

export function isSessionExpired(userData: any) {
  return !userData || !userData.identity || new Date(userData.identity.expiration) < new Date()
}
