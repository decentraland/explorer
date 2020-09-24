import { ConnectorInterface } from './ConnectorInterface'
import { Bitski } from 'bitski'

export class BitskiConnector implements ConnectorInterface {
  private bitski: Bitski
  private readonly network: string

  constructor(config: Map<string, string | boolean>) {
    this.network = config.get('network') as string
    if (!config.get('apiKey') || !config.get('redirectUrl')) {
      throw new Error('Invalid config')
    }
    this.bitski = new Bitski(config.get('apiKey') as string, config.get('redirectUrl') as string)
  }

  getProvider() {
    return this.bitski.getProvider({ networkName: this.network })
  }

  async login(values: Map<string, string>) {
    return this.bitski.signIn()
  }

  async logout() {
    await this.bitski.signOut()
    return true
  }
}
