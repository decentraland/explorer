import { put, takeLatest, call } from 'redux-saga/effects'
import { createIdentity } from 'eth-crypto'
import { Eth } from 'web3x/eth'
import { Personal } from 'web3x/personal/personal'
import { Account } from 'web3x/account'
import { Authenticator } from 'dcl-crypto'

import { ENABLE_WEB3, WORLD_EXPLORER, PREVIEW, ETHEREUM_NETWORK, getTLD, setNetwork } from 'config'

import { createLogger } from 'shared/logger'
import { awaitWeb3Approval, isSessionExpired, providerFuture, loginCompleted } from 'shared/ethereum/provider'
import { getUserProfile, setLocalProfile } from 'shared/comms/peers'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { AUTH_ERROR_LOGGED_OUT, NETWORK_MISMATCH } from 'shared/loading/types'
import { identifyUser, queueTrackingEvent } from 'shared/analytics'
import { getNetworkFromTLD, getAppNetwork } from 'shared/web3'
import { getNetwork } from 'shared/ethereum/EthereumService'

import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'

import { Session } from '.'
import { ExplorerIdentity } from './types'
import { userAuthentified, LOGOUT, LOGIN, loginCompleted as loginCompletedAction } from './actions'

const logger = createLogger('session: ')

export function* sessionSaga(): any {
  yield call(initializeTos)

  yield takeLatest(LOGIN, login)
  yield takeLatest(LOGOUT, logout)
}

function* initializeTos() {
  const TOS_KEY = 'tos'
  const tosAgreed: boolean = getFromLocalStorage(TOS_KEY) ?? false

  const agreeCheck = document.getElementById('agree-check') as HTMLInputElement | undefined
  if (agreeCheck) {
    agreeCheck.checked = tosAgreed
    // @ts-ignore
    agreeCheck.onchange && agreeCheck.onchange()

    const originalOnChange = agreeCheck.onchange
    agreeCheck.onchange = (e) => {
      saveToLocalStorage(TOS_KEY, agreeCheck.checked)
      // @ts-ignore
      originalOnChange && originalOnChange(e)
    }

    // enable agree check after initialization
    enableLogin()
  }
}

function* login() {
  console['group']('connect#login')

  let userId: string
  let identity: ExplorerIdentity

  if (ENABLE_WEB3) {
    yield awaitWeb3Approval()

    if (WORLD_EXPLORER && (yield checkTldVsNetwork())) {
      throw new Error('Network mismatch')
    }

    if (PREVIEW && ETHEREUM_NETWORK.MAINNET === (yield getNetworkValue())) {
      showNetworkWarning()
    }

    try {
      const userData = getUserProfile()

      // check that user data is stored & key is not expired
      if (isSessionExpired(userData)) {
        identity = yield createAuthIdentity()
        userId = identity.address

        setLocalProfile(userId, {
          userId,
          identity
        })
      } else {
        identity = userData.identity
        userId = userData.identity.address

        setLocalProfile(userId, {
          userId,
          identity
        })
      }
    } catch (e) {
      logger.error(e)
      console['groupEnd']()
      ReportFatalError(AUTH_ERROR_LOGGED_OUT)
      throw e
    }

    if (identity.hasConnectedWeb3) {
      identifyUser(userId)
    }
  } else {
    logger.log(`Using test user.`)
    identity = yield createAuthIdentity()
    userId = identity.address

    setLocalProfile(userId, {
      userId,
      identity
    })
  }

  logger.log(`User ${userId} logged in`)

  console['groupEnd']()

  console['group']('connect#ethereum')

  let net: ETHEREUM_NETWORK = ETHEREUM_NETWORK.MAINNET
  if (WORLD_EXPLORER) {
    net = yield getAppNetwork()

    // Load contracts from https://contracts.decentraland.org
    yield setNetwork(net)
    queueTrackingEvent('Use network', { net })
  }

  yield put(userAuthentified(userId, identity, net))

  yield loginCompleted
  yield put(loginCompletedAction())

  console['groupEnd']()
}

async function checkTldVsNetwork() {
  const web3Net = await getNetworkValue()

  const tld = getTLD()
  const tldNet = getNetworkFromTLD()

  if (tld === 'localhost') {
    // localhost => allow any network
    return false
  }

  if (tldNet !== web3Net) {
    document.getElementById('tld')!.textContent = tld
    document.getElementById('web3Net')!.textContent = web3Net
    document.getElementById('web3NetGoal')!.textContent = tldNet

    ReportFatalError(NETWORK_MISMATCH)
    return true
  }

  return false
}

async function getNetworkValue() {
  const web3Network = await getNetwork()
  const web3Net = web3Network === '1' ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN
  return web3Net
}

function showNetworkWarning() {
  const element = document.getElementById('network-warning')
  if (element) {
    element.style.display = 'block'
  }
}

async function createAuthIdentity() {
  const ephemeral = createIdentity()

  const ephemeralLifespanMinutes = 7 * 24 * 60 // 1 week

  let address
  let signer
  let hasConnectedWeb3 = false

  if (ENABLE_WEB3) {
    const result = await providerFuture
    if (result.successful) {
      const eth = Eth.fromCurrentProvider()!
      const account = (await eth.getAccounts())[0]

      address = account.toJSON()
      signer = async (message: string) => {
        let result
        while (!result) {
          try {
            result = await new Personal(eth.provider).sign(message, account, '')
          } catch (e) {
            if (e.message && e.message.includes('User denied message signature')) {
              showEthSignAdvice(true)
            }
          }
        }
        showEthSignAdvice(false)
        return result
      }
      hasConnectedWeb3 = true
    } else {
      const account: Account = result.localIdentity

      address = account.address.toJSON()
      signer = async (message: string) => account.sign(message).signature
    }
  } else {
    const account = Account.create()

    address = account.address.toJSON()
    signer = async (message: string) => account.sign(message).signature
  }

  const auth = await Authenticator.initializeAuthChain(address, ephemeral, ephemeralLifespanMinutes, signer)
  const identity: ExplorerIdentity = { ...auth, address: address.toLocaleLowerCase(), hasConnectedWeb3 }

  return identity
}

function showEthSignAdvice(show: boolean) {
  const element = document.getElementById('eth-sign-advice')
  if (element) {
    element.style.display = show ? 'block' : 'none'
  }
}

function* logout() {
  Session.current.then((s) => s.logout()).catch((e) => logger.error('error while logging out', e))
}

function enableLogin() {
  const wrapper = document.getElementById('eth-login-confirmation-wrapper')
  const spinner = document.getElementById('eth-login-confirmation-spinner')
  if (wrapper && spinner) {
    spinner.style.cssText = 'display: none;'
    wrapper.style.cssText = 'display: flex;'
  }
}
