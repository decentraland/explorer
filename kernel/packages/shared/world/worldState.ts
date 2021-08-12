import { Observable } from 'mz-observable'
import { store } from 'shared/store/isolatedStore'
import { LoadingState } from 'shared/loading/reducer'
import { RendererState } from 'shared/renderer/types'
import { SessionState } from 'shared/session/types'

let hidden: 'hidden' | 'msHidden' | 'webkitHidden' = 'hidden'
let visibilityChange: 'visibilitychange' | 'msvisibilitychange' | 'webkitvisibilitychange' = 'visibilitychange'

if (typeof (document as any).hidden !== 'undefined') {
  // Opera 12.10 and Firefox 18 and later support
  hidden = 'hidden'
  visibilityChange = 'visibilitychange'
} else if (typeof (document as any).msHidden !== 'undefined') {
  hidden = 'msHidden'
  visibilityChange = 'msvisibilitychange'
} else if (typeof (document as any).webkitHidden !== 'undefined') {
  hidden = 'webkitHidden'
  visibilityChange = 'webkitvisibilitychange'
}

export const renderStateObservable = new Observable<void>()
export const foregroundChangeObservable = new Observable<void>()

function handleVisibilityChange() {
  foregroundChangeObservable.notifyObservers()
}

if (hidden && visibilityChange) {
  document.addEventListener(visibilityChange, handleVisibilityChange, false)
}

export function observeLoadingStateChange(onLoadingChange: (previous: LoadingState, current: LoadingState) => any) {
  let previousState = store.getState().loading

  store.subscribe(() => {
    const currentState = store.getState().loading
    if (previousState !== currentState) {
      previousState = currentState
      onLoadingChange(previousState, currentState)
    }
  })
}

export function observeSessionStateChange(onLoadingChange: (previous: SessionState, current: SessionState) => any) {
  let previousState = store.getState().session

  store.subscribe(() => {
    const currentState = store.getState().session
    if (previousState !== currentState) {
      previousState = currentState
      onLoadingChange(previousState, currentState)
    }
  })
}

export function observeRendererStateChange(onLoadingChange: (previous: RendererState, current: RendererState) => any) {
  let previousState = store.getState().renderer

  store.subscribe(() => {
    const currentState = store.getState().renderer
    if (previousState !== currentState) {
      previousState = currentState
      onLoadingChange(previousState, currentState)
    }
  })
}

export function isRendererEnabled(): boolean {
  return store.getState().loading.renderingActivated
}

export function isForeground(): boolean {
  return !(document as any)[hidden]
}

export async function ensureRendererEnabled() {
  if (isRendererEnabled()) {
    return
  }

  return new Promise<void>((resolve) => onNextRendererEnabled(resolve))
}

function onNextRendererEnabled(callback: Function) {
  const observer = renderStateObservable.add(() => {
    if (isRendererEnabled()) {
      renderStateObservable.remove(observer)
      callback()
    }
  })
}
