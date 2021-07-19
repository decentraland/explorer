import { store } from 'shared/store/store'
import { browserInterface } from 'unity-interface/BrowserInterface'
import { RendererInterfaces } from 'unity-interface/dcl'
import { unityInterface } from 'unity-interface/UnityInterface'
import { isInitialized } from './selectors'

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
