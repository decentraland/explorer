import { Eth } from 'web3x/eth'
import { ETHEREUM_NETWORK, ethereumConfigurations, getTLD } from '../../config'
import { WebSocketProvider } from 'eth-connect'
import { ConnectorFactory } from './connector/ConnectorFactory'
import { ProviderType } from './ProviderType'
import { ConnectorInterface } from './connector/ConnectorInterface'

export class Web3Connector {
  private factory: ConnectorFactory
  private connector: ConnectorInterface | undefined
  private readonly network: 'mainnet' | 'ropsten'

  constructor() {
    this.network = getTLD() === 'zone' ? ETHEREUM_NETWORK.ROPSTEN : ETHEREUM_NETWORK.MAINNET
    this.factory = new ConnectorFactory()
  }

  static createWebSocketProvider() {
    const network = getTLD() === 'zone' ? ETHEREUM_NETWORK.ROPSTEN : ETHEREUM_NETWORK.MAINNET
    return new WebSocketProvider(ethereumConfigurations[network].wss)
  }

  async connect(type: ProviderType) {
    try {
      this.connector = this.factory.create(type, this.network)
      await this.connector.login()
      return this.connector.getProvider()
    } catch (e) {
      throw e
    }
  }

  createEth(provider: any = false): Eth | undefined {
    if (provider) {
      return new Eth(provider)
    }
    if (!this.connector) {
      return undefined
    }
    if (this.connector.getProvider().isMetaMask) {
      return Eth.fromCurrentProvider()
    }
    return new Eth(this.connector.getProvider())
  }
}
