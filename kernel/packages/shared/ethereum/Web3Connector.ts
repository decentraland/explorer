import { Eth } from 'web3x/eth'
import { ETHEREUM_NETWORK, ethereumConfigurations, getTLD } from '../../config'
import { WebSocketProvider } from 'eth-connect'
import { ConnectorFactory } from './connector/ConnectorFactory'
import { ProviderType } from './ProviderType'
import { ConnectorInterface } from './connector/ConnectorInterface'

// @todo replace before merge
const API_KEYS = new Map<string, Map<string, string>>([
  [
    ETHEREUM_NETWORK.ROPSTEN,
    new Map([
      [ProviderType.BITSKI, 'd80b4c96-1f1d-4c49-8769-3d429fd390e7'],
      [ProviderType.FORTMATIC, 'pk_test_A8AD7DB2F40251E7'],
      [ProviderType.FORTMATIC_SDK, 'pk_test_A8AD7DB2F40251E7'],
      [ProviderType.MAGIC_LINK, 'pk_test_B4208966E2CB3C25'],
      [ProviderType.PORTIS, '10c37130-32a2-4642-b4b3-96c217aceea7']
    ])
  ],
  [
    ETHEREUM_NETWORK.MAINNET,
    new Map([
      [ProviderType.BITSKI, 'd80b4c96-1f1d-4c49-8769-3d429fd390e7'],
      [ProviderType.FORTMATIC, 'pk_live_6CC35650CE445EFE'],
      [ProviderType.FORTMATIC_SDK, 'pk_live_6CC35650CE445EFE'],
      [ProviderType.MAGIC_LINK, 'pk_live_FC1BEF63E1E562FA'],
      [ProviderType.PORTIS, '10c37130-32a2-4642-b4b3-96c217aceea7']
    ])
  ]
])

export class Web3Connector {
  private factory: ConnectorFactory
  private connector: ConnectorInterface | undefined
  private readonly network: 'mainnet' | 'ropsten'

  constructor() {
    this.network = getTLD() === 'zone' ? ETHEREUM_NETWORK.ROPSTEN : ETHEREUM_NETWORK.MAINNET
    this.factory = new ConnectorFactory(API_KEYS.get(this.network) as Map<string, string>)
  }

  static createWebSocketProvider() {
    const network = getTLD() === 'zone' ? ETHEREUM_NETWORK.ROPSTEN : ETHEREUM_NETWORK.MAINNET
    return new WebSocketProvider(ethereumConfigurations[network].wss)
  }

  async connect(type: ProviderType, values: Map<string, string>) {
    try {
      this.connector = this.factory.create(type, this.network)
      await this.connector.login(values)
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
    return new Eth(this.connector.getProvider())
  }
}
