import { Store } from 'redux'

import { RootState, StoreContainer } from 'shared/store/rootTypes'

import { isInitialized } from './selectors'
import { RendererInterfaces } from 'unity-interface/dcl'

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

  const instancedJS = store.getState().renderer.instancedJS
  if (instancedJS) {
    return instancedJS
  }

  return new Promise<RendererInterfaces>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const instancedJS = store.getState().renderer.instancedJS
      if (instancedJS) {
        unsubscribe()
        return resolve(instancedJS)
      }
    })
  })
}
