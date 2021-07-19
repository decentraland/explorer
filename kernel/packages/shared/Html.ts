import { loadingTips } from './loading/types'
import { future, IFuture } from 'fp-future'
import { LoadingState } from './loading/reducer'
import { ENABLE_WEB3, PREVIEW } from '../config'

const isReact = !!(window as any).reactVersion
const loadingImagesCache: Record<string, IFuture<string>> = {}

export default class Html {
  static showEthLogin() {
    if (isReact) return
    const element = document.getElementById('eth-login')
    if (element) {
      element.style.display = 'block'
    }
  }

  static hideEthLogin() {
    if (isReact) return
    const element = document.getElementById('eth-login')
    if (element) {
      element.style.display = 'none'
    }
  }

  static hideProgressBar() {
    if (isReact) return
    const progressBar = document.getElementById('progress-bar')
    progressBar!.setAttribute('style', 'display: none !important')
  }

  static showErrorModal(targetError: string) {
    if (isReact) return
    document.getElementById('error-' + targetError)!.setAttribute('style', 'display: block !important')
  }

  static hideLoadingTips() {
    if (isReact) return
    const messages = document.getElementById('load-messages')
    const images = document.getElementById('load-images') as HTMLImageElement | null

    if (messages) {
      messages.style.cssText = 'display: none;'
    }
    if (images) {
      images.style.cssText = 'display: none;'
    }
  }

  static cleanSubTextInScreen() {
    if (isReact) return
    const subMessages = document.getElementById('subtext-messages')
    if (subMessages) {
      subMessages.innerText = 'Loading scenes...'
    }
    const progressBar = document.getElementById('progress-bar-inner')
    if (progressBar) {
      progressBar.style.cssText = `width: 0%`
    }
  }

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

  static showEthSignAdvice(show: boolean) {
    if (isReact) return
    showElementById('eth-sign-advice', show)
  }

  static showNetworkWarning() {
    if (isReact || (PREVIEW && !ENABLE_WEB3)) return
    const element = document.getElementById('network-warning')
    if (element) {
      element.style.display = 'block'
    }
  }

  static setLoadingScreen(shouldShow: boolean) {
    if (isReact) return
    document.getElementById('overlay')!.style.display = shouldShow ? 'block' : 'none'
    document.getElementById('load-messages-wrapper')!.style.display = shouldShow ? 'flex' : 'none'
    document.getElementById('progress-bar')!.style.display = shouldShow ? 'block' : 'none'
    const loadingAudio = document.getElementById('loading-audio') as HTMLAudioElement

    if (shouldShow) {
      loadingAudio &&
        loadingAudio.play().catch((e) => {
          /*Ignored. If this fails is not critical*/
        })
    } else {
      loadingAudio && loadingAudio.pause()
    }
  }

  static loopbackAudioElement() {
    return document.getElementById('voice-chat-audio') as HTMLAudioElement | undefined
  }

  static showTeleportAnimation() {
    document
      .getElementById('gameContainer')!
      .setAttribute(
        'style',
        'background: #151419 url(images/teleport.gif) no-repeat center !important; background-size: 194px 257px !important;'
      )
    document.body.setAttribute(
      'style',
      'background: #151419 url(images/teleport.gif) no-repeat center !important; background-size: 194px 257px !important;'
    )
  }

  static hideTeleportAnimation() {
    document.getElementById('gameContainer')!.setAttribute('style', 'background: #151419')
    document.body.setAttribute('style', 'background: #151419')
  }
}

function showElementById(id: string, show: boolean, force: boolean = false) {
  if (isReact && !force) return
  const element = document.getElementById(id)
  if (element) {
    element.style.display = show ? 'block' : 'none'
  }
}
