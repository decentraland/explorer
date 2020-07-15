import { Store } from 'redux'

import { RootState, StoreContainer } from 'shared/store/rootTypes'

import { isInitialized } from './selectors'

declare const globalThis: StoreContainer

export function rendererInitialized() {
  const store: Store<RootState> = globalThis.globalStore

  const initialized = isInitialized(store.getState())
  if (initialized) {
    return Promise.resolve()
  }

  return new Promise((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const initialized = isInitialized(store.getState())
      if (initialized) {
        unsubscribe()
        return resolve()
      }
    })
  })
}

export function rendererEnabled(): Promise<void> {
  const store: Store<RootState> = globalThis.globalStore

  const instancedJS = store.getState().renderer.instancedJS
  if (instancedJS) {
    return Promise.resolve()
  }

  return new Promise((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const instancedJS = store.getState().renderer.instancedJS
      if (instancedJS) {
        unsubscribe()
        return resolve()
      }
    })
  })
}
