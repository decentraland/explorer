import { setLoadingScreenVisible } from 'unity-interface/dcl'

import { disconnect, sendToMordor } from 'shared/comms'
import { bringDownClientAndShowError } from 'shared/loading/ReportFatalError'
import { NEW_LOGIN } from 'shared/loading/types'
import { StoreContainer, RootState } from 'shared/store/rootTypes'

import { getCurrentIdentity, hasWallet as hasWalletSelector } from './selectors'
import { Store } from 'redux'
import { getFromLocalStorage, removeFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { StoredSession } from './types'

declare const globalThis: StoreContainer

// Please note that dcl-profile in local storage does NOT contain profile information anymore. It just contains identity information
// Eventually we could migrate this variable to a new one, if we find this too confusing.
export const getStoredSession: () => StoredSession = () => getFromLocalStorage('dcl-profile') || {}

export const setStoredSession: (session: StoredSession) => void = (session) =>
  saveToLocalStorage('dcl-profile', session)

export const removeStoredSession = () => removeFromLocalStorage('dcl-profile')
export class Session {
  private static _instance: Session = new Session()

  static get current() {
    return Session._instance
  }

  async logout() {
    setLoadingScreenVisible(true)
    sendToMordor()
    disconnect()
    removeStoredSession()
    window.location.reload()
  }

  disable() {
    bringDownClientAndShowError(NEW_LOGIN)
    sendToMordor()
    disconnect()
  }
}

export const getIdentity = () => getCurrentIdentity(globalThis.globalStore.getState())

export const hasWallet = () => hasWalletSelector(globalThis.globalStore.getState())

export async function userAuthentified(): Promise<void> {
  const store: Store<RootState> = globalThis.globalStore

  const initialized = store.getState().session.initialized
  if (initialized) {
    return Promise.resolve()
  }

  return new Promise((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const initialized = store.getState().session.initialized
      if (initialized) {
        unsubscribe()
        return resolve()
      }
    })
  })
}
