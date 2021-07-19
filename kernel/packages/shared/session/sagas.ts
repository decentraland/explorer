import { call, delay, put, select, takeEvery, takeLatest } from 'redux-saga/effects'
import { createIdentity } from 'eth-crypto'
import { Account } from 'web3x/account'
import { Authenticator } from 'dcl-crypto'

import { ENABLE_WEB3, ETHEREUM_NETWORK, PREVIEW, setNetwork, WORLD_EXPLORER } from 'config'

import { createLogger } from 'shared/logger'
import { initializeReferral, referUser } from 'shared/referral'
import { getUserAccount, isSessionExpired, requestManager } from 'shared/ethereum/provider'
import { setLocalInformationForComms } from 'shared/comms/peers'
import { BringDownClientAndShowError, ErrorContext, ReportFatalError } from 'shared/loading/ReportFatalError'
import {
  AUTH_ERROR_LOGGED_OUT,
  AWAITING_USER_SIGNATURE,
  setLoadingScreen,
  setLoadingWaitTutorial
} from 'shared/loading/types'
import { trackEvent } from 'shared/analytics'
import { getAppNetwork } from 'shared/web3'
import { connection } from 'decentraland-connect'

import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'

import { getLastSessionByProvider, getStoredSession, Session, setStoredSession } from './index'
import { ExplorerIdentity, StoredSession } from './types'
import {
  AUTHENTICATE,
  changeLoginStage,
  INIT_SESSION,
  loginCompleted as loginCompletedAction,
  LOGOUT,
  REDIRECT_TO_SIGN_UP,
  SIGNUP,
  SIGNUP_CANCEL,
  signUpClearData,
  signUpSetIdentity,
  signUpSetIsSignUp,
  signUpSetProfile,
  UPDATE_TOS,
  updateTOS,
  userAuthentified,
  AuthenticateAction
} from './actions'
import Html from '../Html'
import { fetchProfileLocally, doesProfileExist } from '../profiles/sagas'
import { generateRandomUserProfile } from '../profiles/generateRandomUserProfile'
import { unityInterface } from '../../unity-interface/UnityInterface'
import { getSignUpIdentity, getSignUpProfile, getIsGuestLogin } from './selectors'
import { ensureRealmInitialized } from '../dao/sagas'
import { saveProfileRequest } from '../profiles/actions'
import { Profile } from '../profiles/types'
import { ensureUnityInterface } from '../renderer'
import { LoginState } from '@dcl/kernel-interface'
import { RequestManager } from 'eth-connect'
import { RootState } from 'shared/store/rootTypes'

const TOS_KEY = 'tos'
const logger = createLogger('session: ')

export function* sessionSaga(): any {
  yield call(initialize)
  yield call(initializeReferral)

  yield takeEvery(UPDATE_TOS, updateTermOfService)
  yield takeLatest(INIT_SESSION, initSession)
  yield takeLatest(LOGOUT, logout)
  yield takeLatest(REDIRECT_TO_SIGN_UP, redirectToSignUp)
  yield takeLatest(SIGNUP, signUp)
  yield takeLatest(SIGNUP_CANCEL, cancelSignUp)
  yield takeLatest(AUTHENTICATE, authenticate)
  yield takeLatest(AWAITING_USER_SIGNATURE, scheduleAwaitingSignaturePrompt)
}

function* initialize() {
  const tosAgreed: boolean = !!getFromLocalStorage(TOS_KEY)
  yield put(updateTOS(tosAgreed))
}

function* updateTermOfService(action: any) {
  return saveToLocalStorage(TOS_KEY, action.payload)
}

function* scheduleAwaitingSignaturePrompt() {
  yield delay(10000)
  const isStillWaiting: boolean = yield select((state: RootState) => !state.session?.identity)

  if (isStillWaiting) {
    yield put(changeLoginStage(LoginState.SIGNATURE_PENDING))
  }
}

function* initSession() {
  yield ensureRealmInitialized()

  // TODO: Move this to website

  yield put(changeLoginStage(LoginState.WAITING_PROVIDER))

  // TODO: if isConnected, connect
}

function* authenticate(action: AuthenticateAction) {
  requestManager.setProvider(action.payload.provider)

  const identity: ExplorerIdentity = yield authorize(requestManager, action.payload.isGuest)
  const profileExists: boolean = yield doesProfileExist(identity.address)
  const isGuestWithProfileLocal: boolean = yield call(isGuestWithProfile, identity)

  yield ensureUnityInterface()

  if (profileExists || isGuestWithProfileLocal || PREVIEW) {
    yield signIn(identity)
  } else {
    yield startSignUp(identity)
  }
}

function* isGuestWithProfile(identity: ExplorerIdentity) {
  const profile = fetchProfileLocally(identity.address)
  const guest: boolean = yield select(getIsGuestLogin)
  return guest && !!profile
}

function* startSignUp(identity: ExplorerIdentity) {
  yield put(signUpSetIsSignUp(true))

  let cachedProfile = fetchProfileLocally(identity.address)
  let profile: Profile = cachedProfile ? cachedProfile : yield generateRandomUserProfile(identity.address)
  profile.userId = identity.address
  profile.ethAddress = identity.rawAddress
  profile.unclaimedName = '' // clean here to allow user complete in passport step
  profile.hasClaimedName = false
  profile.version = 0

  yield put(signUpSetIdentity(identity))
  yield put(signUpSetProfile(profile))

  if (cachedProfile) {
    return yield signUp()
  }

  yield showAvatarEditor()
}

function* showAvatarEditor() {
  const profile: Partial<Profile> = yield select(getSignUpProfile)

  // TODO: Fix as any
  unityInterface.LoadProfile(profile as any)
  unityInterface.ShowAvatarEditorInSignIn()
}

function* authorize(requestManager: RequestManager, isGuest: boolean) {
  if (ENABLE_WEB3) {
    try {
      let userData: StoredSession | null = null

      if (isGuest) {
        userData = getLastSessionByProvider(null)
      } else {
        try {
          const address: string = yield getUserAccount(requestManager, false)
          if (address) {
            userData = getStoredSession(address)

            if (userData) {
              // We save the raw ethereum address of the current user to avoid having to convert-back later after lowercasing it for the userId
              userData.identity.rawAddress = address
            }
          }
        } catch {}
      }

      // check that user data is stored & key is not expired
      if (!userData || isSessionExpired(userData)) {
        const identity: ExplorerIdentity = yield createAuthIdentity(requestManager, isGuest)
        return identity
      }

      return userData.identity
    } catch (e) {
      logger.error(e)
      ReportFatalError(e, ErrorContext.KERNEL_INIT)
      BringDownClientAndShowError(AUTH_ERROR_LOGGED_OUT)
      throw e
    }
  } else {
    logger.log(`Using test user.`)
    const identity: ExplorerIdentity = yield createAuthIdentity(requestManager, isGuest)
    saveSession(identity)
    return identity
  }
}

function* signIn(identity: ExplorerIdentity) {
  logger.log(`User ${identity.address} logged in`)

  yield put(changeLoginStage(LoginState.COMPLETED))

  saveSession(identity)
  if (identity.hasConnectedWeb3) {
    referUser(identity)
  }

  yield setUserAuthentified(identity)

  yield put(loginCompletedAction())
}

function* setUserAuthentified(identity: ExplorerIdentity) {
  let net: ETHEREUM_NETWORK = ETHEREUM_NETWORK.MAINNET
  if (WORLD_EXPLORER) {
    net = yield getAppNetwork()

    // Load contracts from https://contracts.decentraland.org
    yield setNetwork(net)
    trackEvent('Use network', { net })
  }

  yield put(userAuthentified(identity, net))
}

function* signUp() {
  yield put(setLoadingScreen(true))
  const identity: ExplorerIdentity = yield select(getSignUpIdentity)

  if (!identity) {
    debugger
    throw new Error('missing signup session')
  }

  logger.log(`User ${identity.address} signed up`)

  const profile: Partial<Profile> = yield select(getSignUpProfile)
  profile.userId = identity.address
  profile.ethAddress = identity.rawAddress
  profile.version = 0
  profile.hasClaimedName = false
  if (profile.email) {
    profile.tutorialStep = profile.tutorialStep || 0
    profile.tutorialStep |= 128 // We use binary 256 for tutorial and 128 for email promp
  }
  delete profile.email // We don't deploy the email because it is public

  yield put(setLoadingWaitTutorial(true))
  yield signIn(identity)
  yield put(saveProfileRequest(profile))
  yield put(signUpClearData())
}

function* cancelSignUp() {
  yield put(signUpClearData())
  yield put(signUpSetIsSignUp(false))
  yield put(changeLoginStage(LoginState.WAITING_PROVIDER))
}

function saveSession(identity: ExplorerIdentity) {
  const userId = identity.address

  setStoredSession({
    identity
  })

  setLocalInformationForComms(userId, {
    userId,
    identity
  })
}

async function getSigner(
  requestManager: RequestManager,
  isGuest: boolean
): Promise<{
  hasConnectedWeb3: boolean
  address: string
  signer: (message: string) => Promise<string>
  ephemeralLifespanMinutes: number
}> {
  if (ENABLE_WEB3 && !isGuest) {
    const address = await getUserAccount(requestManager, false)

    if (!address) throw new Error("Couldn't get an address from the Ethereum provider")

    return {
      address,
      async signer(message: string) {
        while (true) {
          try {
            let result = await requestManager.personal_sign(message, address, '')
            if (!result) continue
            return result
          } catch (e) {
            if (e.message && e.message.includes('User denied message signature')) {
              put(changeLoginStage(LoginState.SIGNATURE_FAILED))
              Html.showEthSignAdvice(true)
            }
          }
        }
      },
      hasConnectedWeb3: true,
      ephemeralLifespanMinutes: 7 * 24 * 60 // 1 week
    }
  } else {
    const account: Account = Account.create()

    return {
      address: account.address.toJSON().toLowerCase(),
      async signer(message) {
        return account.sign(message).signature
      },
      hasConnectedWeb3: false,
      // If we are using a local profile, we don't want the identity to expire.
      // Eventually, if a wallet gets created, we can migrate the profile to the wallet.
      ephemeralLifespanMinutes: 365 * 24 * 60 * 99
    }
  }
}

async function createAuthIdentity(requestManager: RequestManager, isGuest: boolean): Promise<ExplorerIdentity> {
  const ephemeral = createIdentity()

  const { address, signer, hasConnectedWeb3, ephemeralLifespanMinutes } = await getSigner(requestManager, isGuest)

  const auth = await Authenticator.initializeAuthChain(address, ephemeral, ephemeralLifespanMinutes, signer)

  put(changeLoginStage(LoginState.COMPLETED))

  return { ...auth, rawAddress: address, address: address.toLocaleLowerCase(), hasConnectedWeb3 }
}

function* logout() {
  connection.disconnect()
  Session.current.logout().catch((e) => logger.error('error while logging out', e))
}

function* redirectToSignUp() {
  Session.current.redirectToSignUp().catch((e) => logger.error('error while redirecting to sign up', e))
}
