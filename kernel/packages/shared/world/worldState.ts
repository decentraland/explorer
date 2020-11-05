import { Observable } from '../../decentraland-ecs/src/ecs/Observable'
import future, { IFuture } from 'fp-future'

let hidden: 'hidden' | 'msHidden' | 'webkitHidden' = 'hidden'

if (typeof (document as any).hidden !== 'undefined') {
  // Opera 12.10 and Firefox 18 and later support
  hidden = 'hidden'
} else if (typeof (document as any).msHidden !== 'undefined') {
  hidden = 'msHidden'
} else if (typeof (document as any).webkitHidden !== 'undefined') {
  hidden = 'webkitHidden'
}

let rendererEnabled: boolean = false

export const renderStateObservable = new Observable<Readonly<boolean>>()

renderStateObservable.add((state) => {
  rendererEnabled = state
})

export function isRendererEnabled(): boolean {
  return rendererEnabled
}

export function isForeground(): boolean {
  return !(document as any)[hidden]
}

export async function ensureRendererEnabled() {
  const result: IFuture<void> = future()

  if (isRendererEnabled()) {
    result.resolve()
    return result
  }

  onNextRendererEnabled(() => result.resolve())

  return result
}

export function onNextRendererEnabled(callback: Function) {
  const observer = renderStateObservable.add((isRunning) => {
    if (isRunning) {
      renderStateObservable.remove(observer)
      callback()
    }
  })
}
