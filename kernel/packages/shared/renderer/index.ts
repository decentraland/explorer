import 'unity-interface/UnityInterface'
import { store } from 'shared/store/isolatedStore'
import { BrowserInterface, browserInterface } from 'unity-interface/BrowserInterface'
import { getUnityInstance, IUnityInterface } from 'unity-interface/IUnityInterface'
import { isInitialized } from './selectors'

export type RendererInterfaces = {
  unityInterface: IUnityInterface
  browserInterface: BrowserInterface
}

export async function ensureUnityInterface(): Promise<RendererInterfaces> {
  if (isInitialized(store.getState())) {
    return { unityInterface: getUnityInstance(), browserInterface }
  }

  return new Promise<RendererInterfaces>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      if (isInitialized(store.getState())) {
        unsubscribe()
        return resolve({ unityInterface: getUnityInstance(), browserInterface })
      }
    })
  })
}
