import { RequestManager } from 'eth-connect'
import { Store } from 'redux'
import { accountStateObservable } from 'shared/observables'
import { RootState } from 'shared/store/rootTypes'
import { SessionState } from 'shared/session/types'
import { store } from 'shared/store/store'
import { LoginState } from '@dcl/kernel-interface'

export const requestManager = new RequestManager((window as any).ethereum ?? null)

function observeAccountStateChange(
  store: Store<RootState>,
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
  function isLoginCompleted(state: SessionState) {
    return state.identity && state.provider && state.loginState == LoginState.COMPLETED
  }

  const state = store.getState().session

  if (isLoginCompleted(state)) return state

  return new Promise<SessionState>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const state = store.getState().session
      if (isLoginCompleted(state)) {
        unsubscribe()
        return resolve(state)
      }
    })
  })
}

export function initializeSessionObserver() {
  observeAccountStateChange(store, (_, session) => {
    accountStateObservable.notifyObservers({
      hasProvider: false,
      loginStatus: session.loginState!,
      identity: session.identity,
      network: session.network
    })
  })
}

export async function isGuest(): Promise<boolean> {
  return !!(await onLoginCompleted()).isGuestLogin
}

export function isSessionExpired(userData: any) {
  return !userData || !userData.identity || new Date(userData.identity.expiration) < new Date()
}

export async function getUserAccount(
  requestManager: RequestManager,
  returnChecksum: boolean = false
): Promise<string | undefined> {
  try {
    const accounts = await requestManager.eth_accounts()

    if (!accounts || accounts.length === 0) {
      return undefined
    }

    return returnChecksum ? accounts[0] : accounts[0].toLowerCase()
  } catch (error) {
    throw new Error(`Could not access eth_accounts: "${error.message}"`)
  }
}
