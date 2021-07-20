import { Observable } from '../../decentraland-ecs/src/ecs/Observable'
import { store } from 'shared/store/store'
import { LoadingState } from 'shared/loading/reducer'

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

if (hidden && visibilityChange) {
  document.addEventListener(visibilityChange, handleVisibilityChange, false)

  function handleVisibilityChange() {
    foregroundChangeObservable.notifyObservers()
  }
}

export const renderStateObservable = new Observable<void>()
export const foregroundChangeObservable = new Observable<void>()

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
