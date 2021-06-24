import { Store } from "redux"

export type InitializeUnityResult = {
  container: HTMLElement
}

type KernelWebApp = {
  createStore: () => Store<any>
  initWeb: (container: HTMLElement) => Promise<InitializeUnityResult>
  utils: {
    isBadWord: (word: string) => boolean
    filterInvalidNameCharacters: (name: string) => string
    trackEvent: (eventName: string, eventData: any) => void
  }
}

export type Kernel = typeof window & {
  webApp: KernelWebApp
}
