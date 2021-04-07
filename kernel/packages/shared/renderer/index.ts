import { Store } from 'redux'

import { RootState, StoreContainer } from 'shared/store/rootTypes'

import { isInitialized } from './selectors'
import { RendererInterfaces } from 'unity-interface/dcl'
import { unityInterface } from 'unity-interface/UnityInterface'
import { browserInterface } from 'unity-interface/BrowserInterface'

declare const globalThis: StoreContainer

export function rendererInitialized() {
  const store: Store<RootState> = globalThis.globalStore

  const initialized = isInitialized(store.getState())
  if (initialized) {
    return Promise.resolve()
  }

  return new Promise<void>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const initialized = isInitialized(store.getState())
      if (initialized) {
        unsubscribe()
        return resolve()
      }
    })
  })
}

export async function ensureUnityInterface(): Promise<RendererInterfaces> {
  const store: Store<RootState> = globalThis.globalStore

  const { initialized } = store.getState().renderer
  if (initialized) {
    return {
      unityInterface: unityInterface,
      browserInterface: browserInterface
    }
  }

  return new Promise<RendererInterfaces>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const { initialized } = store.getState().renderer
      if (initialized) {
        unsubscribe()
        return resolve({
          unityInterface: unityInterface,
          browserInterface: browserInterface
        })
      }
    })
  })
}
