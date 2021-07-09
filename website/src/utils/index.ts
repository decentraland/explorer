import { ProviderType } from 'decentraland-connect/dist/types'
import { Kernel } from '../components/types'

const kernel = (window as Kernel).webApp
const ethereum = window.ethereum as any

export function filterInvalidNameCharacters(name: string) {
  return kernel.utils.filterInvalidNameCharacters(name)
}

export function isBadWord(name: string) {
  return kernel.utils.isBadWord(name)
}

export function callOnce<T>(fun: () => T): () => T {
  let result: { value: T } | null = null
  return () => {
    if (!result) {
      result = { value: fun() }
    }

    return result.value
  }
}

export const getWalletName = callOnce(() => {
  if (!ethereum) {
    return 'none'
  } else if (ethereum?.isMetaMask) {
    return 'metamask'
  } else if (ethereum?.isDapper) {
    return 'dapper'
  } else if (ethereum?.isCucumber) {
    return 'cucumber'
  } else if (ethereum?.isTrust) {
    return 'trust'
  } else if (ethereum?.isToshi) {
    return 'coinbase'
  } else if (ethereum?.isGoWallet) {
    return 'goWallet'
  } else if (ethereum?.isAlphaWallet) {
    return 'alphaWallet'
  } else if (ethereum?.isStatus) {
    return 'status'
  } else {
    return 'other'
  }
})

export const getWalletProps = callOnce(() => {
  return Object.keys(ethereum || {})
    .filter((prop) => prop.startsWith('is') && typeof ethereum[prop] === 'boolean')
    .join(',')
})

export type TrackEvents = {
  ['open_login_popup']: {}
  ['click_login_button']: {
    provider_type: ProviderType | 'guest'
  }
}

export function track<E extends keyof TrackEvents>(event: E, properties?: TrackEvents[E]) {
  const wallet = getWalletName()
  const walletProps = getWalletProps()
  kernel.utils.trackEvent(event, { wallet, walletProps, ...properties })
}
