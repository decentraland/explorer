import { AnyAction, Store } from 'redux'
import { IFuture, future } from 'fp-future'

import { isMobile } from 'shared/comms/mobile'

import './apis/index'
import { initializeUrlRealmObserver, realmInitialized } from './dao'
import './events'
import { ReportFatalError } from './loading/ReportFatalError'
import { loadingStarted, notStarted, MOBILE_NOT_SUPPORTED } from './loading/types'
import { defaultLogger } from './logger'
import { buildStore } from './store/store'
import { initializeUrlPositionObserver } from './world/positionThings'
import { RootState, StoreContainer } from './store/rootTypes'
import { login } from './session/actions'

export type InitFutures = {
  essentials: IFuture<void>
  realmInitialization: IFuture<void>
  all: IFuture<void>
}

declare const globalThis: StoreContainer

function initEssentials(): [Store<RootState, AnyAction>] {
  const { store, startSagas } = buildStore()
  globalThis.globalStore = store

  startSagas()

  if (isMobile()) {
    const element = document.getElementById('eth-login')
    if (element) {
      element.style.display = 'none'
    }
    ReportFatalError(MOBILE_NOT_SUPPORTED)
    return [store]
  }

  store.dispatch(notStarted())

  console['group']('connect#login')
  store.dispatch(loadingStarted())

  return [store]
}

export function initShared(): InitFutures {
  const futures: InitFutures = { essentials: future(), realmInitialization: future(), all: future() }
  const [store] = initEssentials()

  ;(async function () {
    store.dispatch(login())
    // TODO await loginCompleted

    if (futures.essentials.isPending) {
      futures.essentials.resolve()
    }

    initializeUrlPositionObserver()
    initializeUrlRealmObserver()

    await realmInitialized()
    futures.realmInitialization.resolve()

    defaultLogger.info(`Using Catalyst configuration: `, store.getState().dao)

    return
  })().then(futures.all.resolve, futures.all.reject)

  return futures
}
