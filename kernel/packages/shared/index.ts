import './apis/index'
import './events'

import { BringDownClientAndShowError } from './loading/ReportFatalError'
import { loadingStarted, notStarted, MOBILE_NOT_SUPPORTED, NO_WEBGL_COULD_BE_CREATED } from './loading/types'
import { buildStore, store } from './store/store'
import { initializeUrlPositionObserver } from './world/positionThings'
import { RootState, StoreContainer } from './store/rootTypes'
import { initSession } from './session/actions'
import { initializeUrlIslandObserver } from './comms'
import { initializeUrlRealmObserver } from './dao'
import { isMobile } from './comms/mobile'
import { isWebGLCompatible } from './comms/browser'
import { rendererVisibleObservable } from './observables'
import { initializeSessionObserver } from './ethereum/provider'
import { isRendererEnabled, observeLoadingStateChange, renderStateObservable } from './world/worldState'
import { isLoadingScreenVisible, isRendererVisible } from './loading/selectors'

declare const globalThis: StoreContainer

export function initShared() {
  if (globalThis.globalStore) {
    return
  }
  const { store, startSagas } = buildStore()
  globalThis.globalStore = store

  if (isMobile()) {
    BringDownClientAndShowError(MOBILE_NOT_SUPPORTED)
    return
  }

  if (!isWebGLCompatible()) {
    BringDownClientAndShowError(NO_WEBGL_COULD_BE_CREATED)
    return
  }

  startSagas()

  store.dispatch(notStarted())
  store.dispatch(loadingStarted())

  initializeUrlPositionObserver()
  initializeUrlRealmObserver()
  initializeUrlIslandObserver()
  initializeRendererVisibleObserver()
  initializeSessionObserver()

  store.dispatch(initSession())
}

export function initializeRendererVisibleObserver() {
  let prevValue: string | null = null
  function sendRefreshedValues() {
    const state: RootState = store.getState()

    const valueToSend = {
      loadingScreen: isLoadingScreenVisible(state),
      // the renderer is visible in game_mode and loading_mode
      visible: isRendererVisible(state)
    }

    const curValue = JSON.stringify(valueToSend)

    if (prevValue != curValue) {
      prevValue = curValue
      rendererVisibleObservable.notifyObservers(valueToSend)
    }
  }

  observeLoadingStateChange((prev, actual) => {
    sendRefreshedValues()
  })

  renderStateObservable.add(() => {
    sendRefreshedValues()
  })

  sendRefreshedValues()
}
