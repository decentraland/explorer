import { Store } from 'redux'
import {
  ETHEREUM_NETWORK,
  getLoginConfigurationForCurrentDomain,
  getTLD,
  PREVIEW,
  setNetwork,
  STATIC_WORLD,
  WORLD_EXPLORER
} from '../config'
import { initialize, queueTrackingEvent, identifyUser } from './analytics'
import './apis/index'
import { connect, persistCurrentUser, disconnect } from './comms'
import { isMobile } from './comms/mobile'
import { setLocalProfile } from './comms/peers'
import './events'
import { ReportFatalError } from './loading/ReportFatalError'
import {
  AUTH_ERROR_LOGGED_OUT,
  COMMS_COULD_NOT_BE_ESTABLISHED,
  MOBILE_NOT_SUPPORTED,
  loadingStarted,
  authSuccessful,
  establishingComms,
  commsEstablished,
  commsErrorRetrying,
  notStarted
} from './loading/types'
import { defaultLogger } from './logger'
import { PassportAsPromise } from './passports/PassportAsPromise'
import { Session } from './session/index'
import { RootState } from './store/rootTypes'
import { buildStore } from './store/store'
import { getAppNetwork, initWeb3 } from './web3'
import { initializeUrlPositionObserver } from './world/positionThings'
import { setWorldContext } from './protocol/actions'
import { profileToRendererFormat } from './passports/transformations/profileToRendererFormat'
import { getUserAccount } from './ethereum/EthereumService'

enum AnalyticsAccount {
  PRD = '1plAT9a2wOOgbPCrTaU8rgGUMzgUTJtU',
  DEV = 'a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc'
}

// TODO fill with segment keys and integrate identity server
function initializeAnalytics() {
  const TLD = getTLD()
  switch (TLD) {
    case 'org':
      if (window.location.host === 'explorer.decentraland.org') {
        return initialize(AnalyticsAccount.PRD)
      }
      return initialize(AnalyticsAccount.DEV)
    case 'today':
      return initialize(AnalyticsAccount.DEV)
    case 'zone':
      return initialize(AnalyticsAccount.DEV)
    default:
      return initialize(AnalyticsAccount.DEV)
  }
}

export let globalStore: Store<RootState>

export async function initShared(): Promise<Session | undefined> {
  if (WORLD_EXPLORER) {
    await initializeAnalytics()
  }

  const { store, startSagas, auth } = buildStore({
    ...getLoginConfigurationForCurrentDomain(),
    ephemeralKeyTTL: 24 * 60 * 60 * 1000
  })
  ;(window as any).globalStore = globalStore = store

  if (WORLD_EXPLORER) {
    startSagas()
  }

  if (isMobile()) {
    ReportFatalError(MOBILE_NOT_SUPPORTED)
    return undefined
  }

  store.dispatch(notStarted())

  const session = new Session()

  let userId: string

  console['group']('connect#login')
  store.dispatch(loadingStarted())

  let net: ETHEREUM_NETWORK = ETHEREUM_NETWORK.MAINNET

  if (WORLD_EXPLORER) {
    try {
      await initWeb3()
      net = await getAppNetwork()

      userId = await auth.getUserId()
      identifyUser(userId)
      session.auth = auth
    } catch (e) {
      defaultLogger.error(e)
      console['groupEnd']()
      ReportFatalError(AUTH_ERROR_LOGGED_OUT)
      throw e
    }
  } else {
    defaultLogger.log(`Using test user.`)
    userId = 'email|5cdd68572d5f842a16d6cc17'
  }

  defaultLogger.log(`User ${userId} logged in`)
  store.dispatch(authSuccessful())

  console['groupEnd']()

  console['group']('connect#ethereum')

  queueTrackingEvent('Use network', { net })

  // Load contracts from https://contracts.decentraland.org
  await setNetwork(net)
  console['groupEnd']()

  initializeUrlPositionObserver()

  // DCL Servers connections/requests after this
  if (STATIC_WORLD) {
    return session
  }

  // initialize profile
  console['group']('connect#profile')
  if (!PREVIEW) {
    const profile = await PassportAsPromise(userId)
    setLocalProfile(userId, {
      userId,
      version: profile.version,
      profile: profileToRendererFormat(profile)
    })
    persistCurrentUser({
      userId,
      version: profile.version,
      profile: profileToRendererFormat(profile)
    })
  }
  console['groupEnd']()

  const account = await getUserAccount()

  console['group']('connect#comms')
  store.dispatch(establishingComms())
  const maxAttemps = 5
  for (let i = 1; ; ++i) {
    try {
      defaultLogger.info(`Attempt number ${i}...`)
      const context = await connect(
        userId,
        net,
        auth,
        account
      )
      if (context !== undefined) {
        store.dispatch(setWorldContext(context))
      }
      break
    } catch (e) {
      if (e.message && e.message.startsWith('error establishing comms')) {
        if (i >= maxAttemps) {
          // max number of attemps reached => rethrow error
          defaultLogger.info(`Max number of attemps reached (${maxAttemps}), unsuccessful connection`)
          disconnect()
          ReportFatalError(COMMS_COULD_NOT_BE_ESTABLISHED)
          throw e
        } else {
          // max number of attempts not reached => continue with loop
          store.dispatch(commsErrorRetrying(i))

          if (e.message && e.message.includes('Result of validation challenge is incorrect')) {
            const element = document.getElementById('eth-sign-advice')
            if (element) {
              element.style.display = 'block'
            }
          }
        }
      } else {
        // not a comms issue per se => rethrow error
        defaultLogger.error(`error while trying to establish communications `, e)
        disconnect()
        throw e
      }
    }
  }
  const element = document.getElementById('eth-sign-advice')
  if (element) {
    element.style.display = 'none'
  }
  store.dispatch(commsEstablished())
  console['groupEnd']()

  return session
}
