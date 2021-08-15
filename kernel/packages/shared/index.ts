import './apis/index'
import './events'

import { BringDownClientAndShowError } from './loading/ReportFatalError'
import { loadingStarted, notStarted, MOBILE_NOT_SUPPORTED, NO_WEBGL_COULD_BE_CREATED } from './loading/types'
import { buildStore } from './store/store'
import { initializeUrlPositionObserver } from './world/positionThings'
import { initSession } from './session/actions'
import { initializeUrlIslandObserver } from './comms'
import { initializeUrlRealmObserver } from './dao'
import { isMobile } from './comms/mobile'
import { isWebGLCompatible } from './comms/browser'
import { globalObservable } from './observables'
import { isRendererVisible } from './loading/selectors'
import { RootStore } from './store/rootTypes'
import { initializeSessionObserver } from './session/sagas'
import { hookAnalyticsObservables } from './analytics'

declare const globalThis: { globalStore: RootStore }

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
  initializeRendererVisibleObserver(store)
  initializeSessionObserver()
  hookAnalyticsObservables()

  // Initializes the Session Saga
  store.dispatch(initSession())
}

function observeIsRendererVisibleChanges(store: RootStore, cb: (visible: boolean) => void) {
  let prevValue = isRendererVisible(store.getState())

  cb(prevValue)

  store.subscribe(() => {
    const newValue = isRendererVisible(store.getState())

    if (newValue !== prevValue) {
      prevValue = newValue
      cb(newValue)
    }
  })
}

export function initializeRendererVisibleObserver(store: RootStore) {
  observeIsRendererVisibleChanges(store, (visible: boolean) => {
    console.log('renderer visible', visible)
    globalObservable.emit('rendererVisible', {
      visible
    })
  })
}
