import Web3Modal from 'web3modal'
import * as Fortmatic from 'fortmatic'
import { Eth } from 'web3x/eth'
import { defaultLogger } from '../logger'

export default class Web3Connector {
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
            key: 'pk_test_A8AD7DB2F40251E7'
          }
        }
      }
    })
    this.web3Modal.clearCachedProvider()
  }

  async connect() {
    try {
      this.provider = await this.web3Modal.connect()
      if (this.provider.isMetaMask) {
        // Request account access if needed
        await Promise.all([this.provider.enable(), window && window.ethereum ? window.ethereum.enable() : null])
      }
      return this.provider
    } catch (e) {
      defaultLogger.error(e)
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
