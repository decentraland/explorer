import { call, delay, put, select, takeLatest } from 'redux-saga/effects'
import { createIdentity } from 'eth-crypto'
import { Personal } from 'web3x/personal/personal'
import { Account } from 'web3x/account'
import { Authenticator } from 'dcl-crypto'
import { Eth } from 'web3x/eth'

import { ENABLE_WEB3, ETHEREUM_NETWORK, getTLD, PREVIEW, setNetwork, WORLD_EXPLORER } from 'config'

import { createLogger } from 'shared/logger'
import {
  requestWeb3Provider,
  createWeb3Connector,
  isSessionExpired,
  loginCompleted,
  providerFuture, requestManager, createProvider
} from 'shared/ethereum/provider'
import { getUserProfile, removeUserProfile, setLocalProfile } from 'shared/comms/peers'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import {
  AUTH_ERROR_LOGGED_OUT,
  AWAITING_USER_SIGNATURE,
  awaitingUserSignature,
  NETWORK_MISMATCH
} from 'shared/loading/types'
import { identifyUser, queueTrackingEvent } from 'shared/analytics'
import { getAppNetwork, getNetworkFromTLD } from 'shared/web3'
import { getNetwork } from 'shared/ethereum/EthereumService'

import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'

import { Session } from '.'
import { ExplorerIdentity } from './types'
import {
  LOGIN, LOGIN_GUEST,
  loginCompleted as loginCompletedAction,
  LOGOUT,
  SETUP_WEB3,
  SIGNUP,
  userAuthentified
} from './actions'
import { profileCheckExists } from '../profiles/actions'

const logger = createLogger('session: ')

export function* sessionSaga(): any {
  yield call(initializeTos)

  yield takeLatest(SETUP_WEB3, setupWeb3)
  yield takeLatest(LOGIN, login)
  yield takeLatest(LOGIN_GUEST, loginGuest)
  yield takeLatest(SIGNUP, login) // signup)
  yield takeLatest(LOGOUT, logout)
  yield takeLatest(AWAITING_USER_SIGNATURE, scheduleAwaitingSignaturePrompt)
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

function* scheduleAwaitingSignaturePrompt() {
  yield delay(10000)
  const isStillWaiting = yield select((state) => !state.session?.initialized)

  if (isStillWaiting) {
    showAwaitingSignaturePrompt(true)
  }
}

function* setupWeb3() {
  if (ENABLE_WEB3) {
    const web3Connector = yield createWeb3Connector()
    const userData = getUserProfile()
    if (userData && userData.userId) {
      const exist = yield profileExists(userData.userId)
      if (isSessionExpired(userData) || !exist) {
        removeUserProfile()
        web3Connector.clearCache()
      }
    }
  }
  enableLogin()
}

function* profileExists(userId: string) {
  const profile = yield call(profileCheckExists, userId)
  const profileId = profile && profile.payload && profile.payload.userId ? profile.payload.userId : null
  return userId !== profileId
}

function* login() {
  let userId: string
  let identity: ExplorerIdentity

  if (ENABLE_WEB3) {
    const provider = yield requestWeb3Provider()
    if (!provider) {
      return
    }

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
        yield put(awaitingUserSignature())
        identity = yield createAuthIdentity()
        showAwaitingSignaturePrompt(false)
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

    loginCompleted.resolve()
  }

  logger.log(`User ${userId} logged in`)

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
}

function* loginGuest() {
  requestManager.setProvider(createProvider())
  const identity: ExplorerIdentity = yield createLocalAuthIdentity()
  const userId: string = identity.address

  setLocalProfile(userId, {
    userId,
    identity
  })

  loginCompleted.resolve()

  let net: ETHEREUM_NETWORK = ETHEREUM_NETWORK.MAINNET
  if (WORLD_EXPLORER) {
    net = yield getAppNetwork()

    // Load contracts from https://contracts.decentraland.org
    yield setNetwork(net)
    queueTrackingEvent('Use network', { net })
  }

  yield put(userAuthentified(userId, identity, net))
  yield put(loginCompletedAction())
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

async function createLocalAuthIdentity(): Promise<ExplorerIdentity> {
  const ephemeral = createIdentity()
  const ephemeralLifespanMinutes = 7 * 24 * 60 // 1 week
  const account = Account.create()
  const address = account.address.toJSON()
  const signer = async (message: string) => account.sign(message).signature
  const auth = await Authenticator.initializeAuthChain(address, ephemeral, ephemeralLifespanMinutes, signer)
  return { ...auth, address: address.toLocaleLowerCase(), hasConnectedWeb3: false }
}

function showEthSignAdvice(show: boolean) {
  showElementById('eth-sign-advice', show)
}

function showElementById(id: string, show: boolean) {
  const element = document.getElementById(id)
  if (element) {
    element.style.display = show ? 'block' : 'none'
  }
}

function showAwaitingSignaturePrompt(show: boolean) {
  showElementById('check-wallet-prompt', show)
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
