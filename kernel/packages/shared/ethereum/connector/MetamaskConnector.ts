import { ConnectorInterface } from './ConnectorInterface'

declare var window: Window & {
  ethereum: any
  web3: any
}

export class MetamaskConnector implements ConnectorInterface {
  constructor() {
    if (!window.ethereum.isMetaMask) {
      throw new Error('Provider does not exist')
    }
  }

  getProvider() {
    return window.ethereum
  }

  async login(values: Map<string, string>) {
    return window.ethereum.request({ method: 'eth_requestAccounts' })
  }

  async logout() {
    return true
  }
}
