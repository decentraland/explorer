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
import { Store } from 'redux'
import { rendererVisibleObservable } from './observables'
import { LoadingState } from './loading/reducer'
import { initializeSessionObserver } from './ethereum/provider'
import { isRendererEnabled, renderStateObservable } from './world/worldState'

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

  store.dispatch(initSession())

  initializeUrlPositionObserver()
  initializeUrlRealmObserver()
  initializeUrlIslandObserver()
  initializeRendererVisibleObserver()
  initializeSessionObserver()
}

function observeLoadingStateChange(
  store: Store<RootState>,
  onLoadingChange: (previous: LoadingState, current: LoadingState) => any
) {
  let previousState = store.getState().loading

  store.subscribe(() => {
    const currentState = store.getState().loading
    if (previousState !== currentState) {
      previousState = currentState
      onLoadingChange(previousState, currentState)
    }
  })
}

export function initializeRendererVisibleObserver() {
  function sendRefreshedValues() {
    rendererVisibleObservable.notifyObservers({
      loadingScreen: !!store.getState().loading.showLoadingScreen,
      visible: isRendererEnabled()
    })
  }

  observeLoadingStateChange(store, () => {
    sendRefreshedValues()
  })

  renderStateObservable.add(() => {
    sendRefreshedValues()
  })
}
