import { call, delay, put, select, takeEvery, takeLatest } from 'redux-saga/effects'
import { createIdentity } from 'eth-crypto'
import { Personal } from 'web3x/personal/personal'
import { Account } from 'web3x/account'
import { Authenticator } from 'dcl-crypto'

import { ENABLE_WEB3, ETHEREUM_NETWORK, getTLD, PREVIEW, setNetwork, WORLD_EXPLORER } from 'config'

import { createLogger } from 'shared/logger'
import { initializeReferral, referUser } from 'shared/referral'
import {
  createEth,
  createWeb3Connector,
  getProviderType,
  getUserEthAccountIfAvailable,
  isGuest,
  isSessionExpired,
  loginCompleted,
  providerFuture,
  requestWeb3Provider
} from 'shared/ethereum/provider'
import { setLocalInformationForComms } from 'shared/comms/peers'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import {
  AUTH_ERROR_LOGGED_OUT,
  AWAITING_USER_SIGNATURE,
  awaitingUserSignature,
  NETWORK_MISMATCH,
  setLoadingScreen,
  setTLDError
} from 'shared/loading/types'
import { identifyUser, queueTrackingEvent } from 'shared/analytics'
import { getAppNetwork, getNetworkFromTLD } from 'shared/web3'
import { getNetwork } from 'shared/ethereum/EthereumService'

import { getFromLocalStorage, removeFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'

import { getLastSessionWithoutWallet, getStoredSession, removeStoredSession, Session, setStoredSession } from './index'
import { ExplorerIdentity, LOCAL_GUEST_PROFILE_KEY, LoginStage, SignUpStage } from './types'
import {
  AUTHENTICATE,
  AuthenticateAction,
  changeLoginStage,
  changeSignUpStage,
  INIT_SESSION,
  LOGIN,
  LoginAction,
  loginCompleted as loginCompletedAction,
  LOGOUT,
  signInSetCurrentProvider,
  signInSigning,
  SIGNUP,
  SIGNUP_CANCEL,
  SIGNUP_COME_BACK_TO_AVATAR_EDITOR,
  signUpClearData,
  signUpSetIdentity,
  signUpSetProfile,
  toggleWalletPrompt,
  UPDATE_TOS,
  updateTOS,
  userAuthentified
} from './actions'
import { ProviderType } from '../ethereum/ProviderType'
import Html from '../Html'
import { getProfileByUserId } from '../profiles/sagas'
import { generateRandomUserProfile } from '../profiles/generateRandomUserProfile'
import { unityInterface } from '../../unity-interface/UnityInterface'
import { getSignUpIdentity, getSignUpProfile } from './selectors'
import { ensureRealmInitialized } from '../dao/sagas'
import { ensureBaseCatalogs } from '../catalogs/sagas'
import { saveProfileRequest } from '../profiles/actions'
import { Profile } from '../profiles/types'

const TOS_KEY = 'tos'
const logger = createLogger('session: ')

export function* sessionSaga(): any {
  yield call(initialize)
  yield call(initializeReferral)

  yield takeEvery(UPDATE_TOS, updateTermOfService)
  yield takeLatest(INIT_SESSION, initSession)
  yield takeLatest(LOGIN, login)
  yield takeLatest(LOGOUT, logout)
  yield takeLatest(SIGNUP, signUp)
  yield takeLatest(SIGNUP_CANCEL, cancelSignUp)
  yield takeLatest(AUTHENTICATE, authenticate)
  yield takeLatest(AWAITING_USER_SIGNATURE, scheduleAwaitingSignaturePrompt)
  yield takeLatest(SIGNUP_COME_BACK_TO_AVATAR_EDITOR, showAvatarEditor)
}

function* initialize() {
  const tosAgreed: boolean = !!getFromLocalStorage(TOS_KEY)
  yield put(updateTOS(tosAgreed))
  Html.initializeTos(tosAgreed)
}

function* updateTermOfService(action: any) {
  return saveToLocalStorage(TOS_KEY, action.payload)
}

function* scheduleAwaitingSignaturePrompt() {
  yield delay(10000)
  const isStillWaiting = yield select((state) => !state.session?.initialized)

  if (isStillWaiting) {
    yield put(toggleWalletPrompt(true))
    Html.showAwaitingSignaturePrompt(true)
  }
}

function* initSession() {
  yield ensureRealmInitialized()
  if (ENABLE_WEB3) {
    Html.showEthLogin()
    yield createWeb3Connector()
    yield checkPreviousSession()
  }
  yield put(changeLoginStage(LoginStage.SING_IN))
  Html.bindLoginEvent()
}

function* checkPreviousSession() {
  const session = getLastSessionWithoutWallet()
  if (!isSessionExpired(session) && session) {
    const identity = session.identity
    if (identity?.provider && identity.provider !== ProviderType.GUEST) {
      yield put(signInSetCurrentProvider(identity.provider))
    }
  } else {
    removeStoredSession(session?.userId)
  }
}

function* authenticate(action: AuthenticateAction) {
  yield put(signInSigning(true))
  const provider = yield requestProvider(action.payload.provider as ProviderType)
  if (!provider) {
    yield put(signInSigning(false))
    return
  }
  const session = yield authorize()
  let profile = yield getProfileByUserId(session.userId)
  if (profile) {
    return yield signIn(session.userId, session.identity)
  }
  return yield startSignUp(session.userId, session.identity)
}

function* startSignUp(userId: string, identity: ExplorerIdentity) {
  let prevGuest = isGuest() ? null : getFromLocalStorage(LOCAL_GUEST_PROFILE_KEY)
  let profile: Profile = prevGuest ? prevGuest : yield generateRandomUserProfile(userId)
  profile.userId = identity.address.toString()
  profile.ethAddress = identity.address.toString()
  profile.hasClaimedName = false
  profile.inventory = []
  profile.version = 0

  yield put(signUpSetIdentity(userId, identity))
  yield put(signUpSetProfile(profile))

  if (prevGuest) {
    removeFromLocalStorage(LOCAL_GUEST_PROFILE_KEY)
    return yield signUp()
  }
  yield showAvatarEditor()
}

function* showAvatarEditor() {
  yield put(setLoadingScreen(true))
  yield put(changeLoginStage(LoginStage.SING_UP))
  yield put(changeSignUpStage(SignUpStage.AVATAR))

  const profile = yield select(getSignUpProfile)

  yield ensureBaseCatalogs()
  unityInterface.LoadProfile(profile)
  unityInterface.ShowAvatarEditorInSignIn()
  yield put(setLoadingScreen(false))
  Html.switchGameContainer(true)
}

function* requestProvider(providerType: ProviderType) {
  const provider = yield requestWeb3Provider(providerType)
  if (provider) {
    if (WORLD_EXPLORER && (yield checkTldVsNetwork())) {
      throw new Error('Network mismatch')
    }

    if (PREVIEW && ETHEREUM_NETWORK.MAINNET === (yield getNetworkValue())) {
      Html.showNetworkWarning()
    }
  }
  return provider
}

function* authorize() {
  if (ENABLE_WEB3) {
    try {
      let address = yield getUserEthAccountIfAvailable()
      let userData = address ? getStoredSession(address) : undefined
      // check that user data is stored & key is not expired
      if (!userData || isSessionExpired(userData) || (address && userData.userId !== address.toLowerCase())) {
        const identity = yield createAuthIdentity()
        return {
          userId: identity.address,
          identity
        }
      }
      return {
        userId: userData.identity.address,
        identity: userData.identity
      }
    } catch (e) {
      logger.error(e)
      ReportFatalError(AUTH_ERROR_LOGGED_OUT)
      throw e
    }
  } else {
    logger.log(`Using test user.`)
    const identity = yield createAuthIdentity()
    const profile = { userId: identity.address, identity }
    saveSession(profile.userId, profile.identity)
    return profile
  }
}

function* signIn(userId: string, identity: ExplorerIdentity) {
  logger.log(`User ${userId} logged in`)
  yield put(changeLoginStage(LoginStage.COMPLETED))

  saveSession(userId, identity)
  if (identity.hasConnectedWeb3) {
    identifyUser(userId)
    referUser(identity)
  }

  yield put(signInSigning(false))
  yield setUserAuthentified(userId, identity)

  loginCompleted.resolve()
  yield put(loginCompletedAction())
}

function* setUserAuthentified(userId: string, identity: ExplorerIdentity) {
  let net: ETHEREUM_NETWORK = ETHEREUM_NETWORK.MAINNET
  if (WORLD_EXPLORER) {
    net = yield getAppNetwork()

    // Load contracts from https://contracts.decentraland.org
    yield setNetwork(net)
    queueTrackingEvent('Use network', { net })
  }

  yield put(userAuthentified(userId, identity, net))
}

function* signUp() {
  yield put(setLoadingScreen(true))
  yield put(changeLoginStage(LoginStage.COMPLETED))
  const session = yield select(getSignUpIdentity)

  logger.log(`User ${session.userId} signed up`)

  const profile = yield select(getSignUpProfile)
  profile.userId = session.userId.toString()
  profile.ethAddress = session.userId.toString()
  profile.version = 0
  profile.inventory = []
  profile.tutorialStep = 0
  profile.hasClaimedName = false
  delete profile.email // We don't deploy the email because it is public

  yield signIn(session.userId, session.identity)
  yield put(saveProfileRequest(profile, session.userId))
  yield put(signUpClearData())
  if (isGuest()) {
    saveToLocalStorage(LOCAL_GUEST_PROFILE_KEY, profile)
  }
  unityInterface.ActivateRendering()
}

function* cancelSignUp() {
  yield put(signUpClearData())
  yield put(signInSigning(false))
  yield put(changeLoginStage(LoginStage.SING_IN))
}

function* login(action: LoginAction) {
  let userId: string
  let identity: ExplorerIdentity

  if (ENABLE_WEB3) {
    try {
      if (!(yield requestProvider(action.payload.provider as ProviderType))) {
        yield put(changeLoginStage(LoginStage.CONNECT_ADVICE))
        return
      }
    } catch (e) {
      return
    }
    yield put(changeLoginStage(LoginStage.COMPLETED))
    Html.hideEthLogin()
    try {
      const address = yield getUserEthAccountIfAvailable()
      const userData = address ? getStoredSession(address) : getLastSessionWithoutWallet()

      // check that user data is stored & key is not expired
      if (isSessionExpired(userData)) {
        yield put(awaitingUserSignature())
        identity = yield createAuthIdentity()
        yield put(toggleWalletPrompt(false))
        Html.showAwaitingSignaturePrompt(false)
        userId = identity.address

        saveSession(userId, identity)
      } else {
        identity = userData!.identity
        userId = userData!.identity.address

        saveSession(userId, identity)
      }
    } catch (e) {
      logger.error(e)
      ReportFatalError(AUTH_ERROR_LOGGED_OUT)
      throw e
    }

    if (identity.hasConnectedWeb3) {
      identifyUser(userId)
      referUser(identity)
    }
  } else {
    logger.log(`Using test user.`)
    identity = yield createAuthIdentity()
    userId = identity.address

    saveSession(userId, identity)

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

  loginCompleted.resolve()
  yield put(loginCompletedAction())
}

function saveSession(userId: string, identity: ExplorerIdentity) {
  setStoredSession({
    userId,
    identity
  })

  setLocalInformationForComms(userId, {
    userId,
    identity
  })
}

function* checkTldVsNetwork() {
  const web3Net = yield getNetworkValue()

  const tld = getTLD()
  const tldNet = getNetworkFromTLD()

  if (tld === 'localhost') {
    // localhost => allow any network
    return false
  }

  if (tldNet !== web3Net) {
    yield put(setTLDError({ tld, web3Net, tldNet }))
    Html.updateTLDInfo(tld, web3Net, tldNet as string)
    ReportFatalError(NETWORK_MISMATCH)
    return true
  }

  return false
}

async function getNetworkValue() {
  const web3Network = await getNetwork()
  return web3Network === '1' ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN
}

async function createAuthIdentity(): Promise<ExplorerIdentity> {
  const ephemeral = createIdentity()

  let ephemeralLifespanMinutes = 7 * 24 * 60 // 1 week

  let address
  let signer
  let provider: ProviderType | undefined
  let hasConnectedWeb3 = false

  if (ENABLE_WEB3) {
    const result = await providerFuture
    if (result.successful) {
      const eth = createEth()
      const account = (await eth.getAccounts())[0]

      address = account.toJSON()
      provider = getProviderType()
      signer = async (message: string) => {
        let result
        while (!result) {
          try {
            result = await new Personal(eth.provider).sign(message, account, '')
          } catch (e) {
            if (e.message && e.message.includes('User denied message signature')) {
              put(changeLoginStage(LoginStage.SING_ADVICE))
              Html.showEthSignAdvice(true)
            }
          }
        }
        put(changeLoginStage(LoginStage.COMPLETED))
        Html.showEthSignAdvice(false)
        return result
      }
      hasConnectedWeb3 = true
    } else {
      const account: Account = result.localIdentity

      provider = ProviderType.GUEST
      address = account.address.toJSON()
      signer = async (message: string) => account.sign(message).signature

      // If we are using a local profile, we don't want the identity to expire.
      // Eventually, if a wallet gets created, we can migrate the profile to the wallet.
      ephemeralLifespanMinutes = 365 * 24 * 60 * 99
    }
  } else {
    const account = Account.create()

    provider = ProviderType.GUEST
    address = account.address.toJSON()
    signer = async (message: string) => account.sign(message).signature
  }

  const auth = await Authenticator.initializeAuthChain(address, ephemeral, ephemeralLifespanMinutes, signer)
  return { ...auth, address: address.toLocaleLowerCase(), hasConnectedWeb3, provider }
}

function* logout() {
  Session.current.logout().catch((e) => logger.error('error while logging out', e))
}
