import { store } from 'shared/store/store'
import { BrowserInterface, browserInterface } from 'unity-interface/BrowserInterface'
import { UnityInterface, unityInterface } from 'unity-interface/UnityInterface'
import { isInitialized } from './selectors'

export type RendererInterfaces = {
  unityInterface: UnityInterface
  browserInterface: BrowserInterface
}

export async function ensureUnityInterface(): Promise<RendererInterfaces> {
  if (isInitialized(store.getState())) {
    return { unityInterface, browserInterface }
  }

  return new Promise<RendererInterfaces>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      if (isInitialized(store.getState())) {
        unsubscribe()
        return resolve({ unityInterface, browserInterface })
      }
    })
  })
}
