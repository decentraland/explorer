import { ConnectorInterface } from './ConnectorInterface'

declare var window: Window & {
  ethereum: any
}

export class DapperConnector implements ConnectorInterface {
  isAvailable() {
    return window['ethereum'] && window.ethereum.isDapper
  }

  getProvider() {
    return window.ethereum
  }

  async login() {
    const accounts = await window.ethereum.enable()
    this.subscribeToChanges()
    return accounts
  }

  async logout() {
    return true
  }

  private subscribeToChanges() {}
}
