import { call, put, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { createIdentity } from 'eth-crypto'
import { Account } from 'web3x/account'
import { Authenticator } from 'dcl-crypto'

import { ETHEREUM_NETWORK } from 'config'

import { createLogger } from 'shared/logger'
import { initializeReferral, referUser } from 'shared/referral'
import { getUserAccount, isSessionExpired, requestManager } from 'shared/ethereum/provider'
import { setLocalInformationForComms } from 'shared/comms/peers'
import { awaitingUserSignature, AWAITING_USER_SIGNATURE, setLoadingWaitTutorial } from 'shared/loading/types'
import { getAppNetwork, registerProviderNetChanges } from 'shared/web3'

import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'

import { getLastGuestSession, getStoredSession, Session, setStoredSession } from './index'
import { ExplorerIdentity, RootSessionState, SessionState, StoredSession } from './types'
import {
  AUTHENTICATE,
  changeLoginState,
  INIT_SESSION,
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
  AuthenticateAction,
  signUpCancel
} from './actions'
import { fetchProfileLocally, doesProfileExist, generateRandomUserProfile } from '../profiles/sagas'
import { getUnityInstance } from '../../unity-interface/IUnityInterface'
import { getIsGuestLogin, getSignUpIdentity, getSignUpProfile, isLoginCompleted } from './selectors'
import { ensureRealmInitialized } from '../dao/sagas'
import { saveProfileRequest } from '../profiles/actions'
import { Profile } from '../profiles/types'
import { ensureUnityInterface } from '../renderer'
import { LoginState } from '@dcl/kernel-interface'
import { RequestManager } from 'eth-connect'
import { ensureMetaConfigurationInitialized } from 'shared/meta'
import { Store } from 'redux'
import { store } from 'shared/store/isolatedStore'
import { globalObservable } from 'shared/observables'
import { selectNetwork } from 'shared/dao/actions'
import { getSelectedNetwork } from 'shared/dao/selectors'

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
  yield takeLatest(AWAITING_USER_SIGNATURE, signaturePrompt)
}

function* initialize() {
  const tosAgreed: boolean = !!getFromLocalStorage(TOS_KEY)
  yield put(updateTOS(tosAgreed))
}

function* updateTermOfService(action: any) {
  return saveToLocalStorage(TOS_KEY, action.payload)
}

function* signaturePrompt() {
  yield put(changeLoginState(LoginState.SIGNATURE_PENDING))
}

function* initSession() {
  yield put(changeLoginState(LoginState.WAITING_PROVIDER))
}

function* authenticate(action: AuthenticateAction) {
  // setup provider
  requestManager.setProvider(action.payload.provider)

  yield put(changeLoginState(LoginState.SIGNATURE_PENDING))

  let identity: ExplorerIdentity

  try {
    identity = yield authorize(requestManager)
  } catch (e) {
    if (('' + (e.message || e.toString())).includes('User denied message signature')) {
      yield put(signUpCancel())
      return
    } else {
      throw e
    }
  }

  yield ensureUnityInterface()

  yield put(changeLoginState(LoginState.WAITING_PROFILE))

  // set the etherum network to start loading profiles
  const net: ETHEREUM_NETWORK = yield call(getAppNetwork)
  yield put(selectNetwork(net))
  registerProviderNetChanges()
  yield call(ensureRealmInitialized)

  const profileExists: boolean = yield doesProfileExist(identity.address)
  const isGuest: boolean = yield select(getIsGuestLogin)
  const isGuestWithProfileLocal: boolean = isGuest && !!fetchProfileLocally(identity.address, net)

  if (profileExists || isGuestWithProfileLocal) {
    yield put(setLoadingWaitTutorial(false))
    yield signIn(identity)
  } else {
    yield startSignUp(identity)
    yield take(SIGNUP)
  }
}

function* startSignUp(identity: ExplorerIdentity) {
  yield put(signUpSetIsSignUp(true))

  const net: ETHEREUM_NETWORK = yield call(getAppNetwork)
  let cachedProfile = fetchProfileLocally(identity.address, net)
  let profile: Profile = cachedProfile ? cachedProfile : yield generateRandomUserProfile(identity.address)
  profile.userId = identity.address
  profile.ethAddress = identity.rawAddress
  profile.unclaimedName = '' // clean here to allow user complete in passport step
  profile.hasClaimedName = false
  profile.version = 0

  yield put(signUpSetIdentity(identity))
  yield put(signUpSetProfile(profile))

  if (cachedProfile) {
    yield signUp()
  } else {
    const profile: Partial<Profile> = yield select(getSignUpProfile)

    // TODO: Fix as any
    getUnityInstance().LoadProfile(profile as any)
    getUnityInstance().ShowAvatarEditorInSignIn()
  }
}

function* authorize(requestManager: RequestManager) {
  let userData: StoredSession | null = null

  const isGuest: boolean = yield select(getIsGuestLogin)

  if (isGuest) {
    userData = getLastGuestSession()
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
    } catch (e) {
      logger.error(e)
      // do nothing
    }
  }

  // check that user data is stored & key is not expired
  if (!userData || isSessionExpired(userData)) {
    yield put(awaitingUserSignature())
    const identity: ExplorerIdentity = yield createAuthIdentity(requestManager, isGuest)
    return identity
  }

  return userData.identity
  // } catch (e) {
  // logger.error(e)
  // ReportFatalError(e, ErrorContext.KERNEL_INIT)
  // BringDownClientAndShowError(AUTH_ERROR_LOGGED_OUT)
  // throw e
  // }
}

function* signIn(identity: ExplorerIdentity) {
  logger.log(`User ${identity.address} logged in`)

  const isGuest: boolean = yield select(getIsGuestLogin)
  saveSession(identity, isGuest)

  if (identity.hasConnectedWeb3) {
    referUser(identity)
  }

  let net: ETHEREUM_NETWORK = yield select(getSelectedNetwork)

  yield ensureMetaConfigurationInitialized()

  yield put(userAuthentified(identity, net))

  yield put(changeLoginState(LoginState.COMPLETED))
}

function* signUp() {
  const identity: ExplorerIdentity = yield select(getSignUpIdentity)

  if (!identity) {
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
  yield put(changeLoginState(LoginState.WAITING_PROVIDER))
}

function saveSession(identity: ExplorerIdentity, isGuest: boolean) {
  const userId = identity.address

  setStoredSession({
    identity,
    isGuest
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
  if (!isGuest) {
    const address = await getUserAccount(requestManager, false)

    if (!address) throw new Error("Couldn't get an address from the Ethereum provider")

    return {
      address,
      async signer(message: string) {
        while (true) {
          let result = await requestManager.personal_sign(message, address, '')
          if (!result) continue
          return result
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

  return { ...auth, rawAddress: address, address: address.toLocaleLowerCase(), hasConnectedWeb3 }
}

function* logout() {
  Session.current.logout().catch((e) => logger.error('error while logging out', e))
}

function* redirectToSignUp() {
  Session.current.redirectToSignUp().catch((e) => logger.error('error while redirecting to sign up', e))
}

export function observeAccountStateChange(
  store: Store<RootSessionState>,
  accountStateChange: (previous: SessionState, current: SessionState) => any
) {
  let previousState = store.getState().session

  store.subscribe(() => {
    const currentState = store.getState().session
    if (previousState !== currentState) {
      previousState = currentState
      accountStateChange(previousState, currentState)
    }
  })
}

export async function onLoginCompleted(): Promise<SessionState> {
  const state = store.getState()

  if (isLoginCompleted(state)) return state.session

  return new Promise<SessionState>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const state = store.getState()
      if (isLoginCompleted(state)) {
        unsubscribe()
        return resolve(state.session)
      }
    })
  })
}

export function initializeSessionObserver() {
  observeAccountStateChange(store, (_, session) => {
    globalObservable.emit('accountState', {
      hasProvider: !!session.provider,
      loginStatus: session.loginState as LoginState,
      identity: session.identity,
      network: session.network,
      isGuest: !!session.isGuestLogin
    })
  })
}
