import Web3Modal from 'web3modal'
import * as Fortmatic from 'fortmatic'
import { Eth } from 'web3x/eth'
import { ETHEREUM_NETWORK, ethereumConfigurations, getTLD } from '../../config'
import { WebSocketProvider } from 'eth-connect'

declare var window: Window & {
  ethereum: any
  web3: any
}

export enum ProviderType {
  METAMASK = 'Metamask',
  FORTMATIC = 'fortmatic'
}

const FORTMATIC_API_KEY = 'pk_test_A8AD7DB2F40251E7'

export class Web3Connector {
  private provider: any
  private readonly web3Modal: Web3Modal

  constructor() {
    this.web3Modal = new Web3Modal({
      network: 'ropsten', // 'mainnet',
      cacheProvider: false,
      providerOptions: {
        fortmatic: {
          package: Fortmatic,
          options: {
            key: FORTMATIC_API_KEY
          }
        }
      }
    })
  }

  static createWebSocketProvider() {
    const network = getTLD() === 'zone' ? ETHEREUM_NETWORK.ROPSTEN : ETHEREUM_NETWORK.MAINNET
    return new WebSocketProvider(ethereumConfigurations[network].wss)
  }

  clearCache() {
    this.web3Modal.clearCachedProvider()
  }

  async connect(type: ProviderType) {
    try {
      console.log('TYPE: ', type)
      this.provider = await this.web3Modal.connect()
      // this.provider = await this.web3Modal.connectTo(type)
      if (this.provider.isMetaMask) {
        // Request account access if needed
        await Promise.all([this.provider.enable(), window && window.ethereum ? window.ethereum.enable() : null])
      }
      return this.provider
    } catch (e) {
      throw e
    }
  }

  createEth(provider: any = false): Eth | undefined {
    if (provider) {
      return new Eth(provider)
    }
    if (!this.provider) {
      return undefined
    }
    if (this.provider.isMetaMask) {
      return Eth.fromCurrentProvider()!
    }
    return new Eth(this.provider)
  }
}
