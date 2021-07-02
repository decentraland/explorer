import { isMobile } from './comms/mobile'
import { isCompatibleBrowser } from './comms/browser'

import './apis/index'
import './events'

import { initializeUrlRealmObserver } from './dao'
import { BringDownClientAndShowError } from './loading/ReportFatalError'
import { loadingStarted, notStarted, MOBILE_NOT_SUPPORTED, NO_WEBGL_COULD_BE_CREATED } from './loading/types'
import { buildStore } from './store/store'
import { initializeUrlPositionObserver } from './world/positionThings'
import { StoreContainer } from './store/rootTypes'
import { initSession } from './session/actions'

declare const globalThis: StoreContainer

export function initShared() {
  if (globalThis.globalStore) {
    return
  }
  const { store, startSagas } = buildStore()
  globalThis.globalStore = store

  startSagas()

  if (isMobile()) {
    BringDownClientAndShowError(MOBILE_NOT_SUPPORTED)
    return
  } else if (!isCompatibleBrowser()) {
    BringDownClientAndShowError(NO_WEBGL_COULD_BE_CREATED)
    return
  }

  store.dispatch(notStarted())
  store.dispatch(loadingStarted())

  store.dispatch(initSession())

  initializeUrlPositionObserver()
  initializeUrlRealmObserver()
}
