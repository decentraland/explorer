import { ConnectorInterface } from './ConnectorInterface'
import { Eth } from 'web3x/eth'

const Portis = require('@portis/web3')

export class PortisConnector implements ConnectorInterface {
  private portis: any

  constructor(config: Map<string, string | boolean>) {
    this.portis = new Portis(config.get('apiKey') as string, config.get('network') as string)
  }

  getProvider(): any {
    return this.portis.provider
  }

  async login(values: Map<string, string>) {
    const eth = new Eth(this.getProvider())
    await eth.getAccounts()
    return this.getProvider()
  }

  async logout() {
    return this.portis.logout()
  }
}
