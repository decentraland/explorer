import { loadingTips } from './loading/types'
import { future, IFuture } from 'fp-future'
import { LoadingState } from './loading/reducer'

const isReact = !!(window as any).reactVersion
const loadingImagesCache: Record<string, IFuture<string>> = {}

export default class Html {
  static async updateTextInScreen(status: LoadingState) {
    if (isReact) return
    const messages = document.getElementById('load-messages')
    const images = document.getElementById('load-images') as HTMLImageElement | null
    if (messages && images) {
      const loadingTip = loadingTips[status.helpText]
      if (messages.innerText !== loadingTip.text) {
        messages.innerText = loadingTip.text
      }

      if (!loadingImagesCache[loadingTip.image]) {
        const promise = (loadingImagesCache[loadingTip.image] = future())
        const response = await fetch(loadingTip.image)
        const blob = await response.blob()
        const url = URL.createObjectURL(blob)
        promise.resolve(url)
      }

      const url = await loadingImagesCache[loadingTip.image]
      if (url !== images.src) {
        images.src = url
      }
    }
    const subMessages = document.getElementById('subtext-messages')
    const progressBar = document.getElementById('progress-bar-inner')
    if (subMessages && progressBar) {
      const newMessage = status.pendingScenes > 0 ? status.message || 'Loading scenes...' : status.status
      if (newMessage !== subMessages.innerText) {
        subMessages.innerText = newMessage
      }
      const actualPercentage = Math.floor(
        Math.min(status.initialLoad ? (status.loadPercentage + status.subsystemsLoad) / 2 : status.loadPercentage, 100)
      )
      const newCss = `width: ${actualPercentage}%`
      if (newCss !== progressBar.style.cssText) {
        progressBar.style.cssText = newCss
      }
    }
  }

  static loopbackAudioElement() {
    return document.getElementById('voice-chat-audio') as HTMLAudioElement | undefined
  }

  static hideTeleportAnimation() {
    document.getElementById('gameContainer')!.setAttribute('style', 'background: #151419')
    document.body.setAttribute('style', 'background: #151419')
  }
}
