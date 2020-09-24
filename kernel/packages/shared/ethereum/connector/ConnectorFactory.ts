import { ProviderType } from '../Web3Connector'
import { FortmaticConnector } from './FortmaticConnector'
import { FortmaticSDKConnector } from './FortmaticSDKConnector'
import { MagicLinkConnector } from './MagicLinkConnector'
import { BitskiConnector } from './BitskiConnector'
import { MetamaskConnector } from './MetamaskConnector'
import { ConnectorInterface } from './ConnectorInterface'
import { PortisConnector } from './PortisConnector'

export class ConnectorFactory {
  private keys: Map<string, string>

  constructor(apiKeys: Map<string, string>) {
    this.keys = apiKeys
  }

  create(type: ProviderType, network: string): ConnectorInterface {
    if (type === ProviderType.METAMASK) {
      return new MetamaskConnector()
    }
    const config = new Map<string, string>()
    config.set('network', network)
    config.set('apiKey', this.keys.get(type) as string)

    if (type === ProviderType.BITSKI) {
      config.set('redirectUrl', `${window.location.origin}/bitski.html`)
      return new BitskiConnector(config)
    }
    if (type === ProviderType.PORTIS) {
      return new PortisConnector(config)
    }
    if (type === ProviderType.MAGIC_LINK) {
      return new MagicLinkConnector(config)
    }
    if (type === ProviderType.FORTMATIC) {
      return new FortmaticConnector(config)
    }
    if (type === ProviderType.FORTMATIC_SDK) {
      return new FortmaticSDKConnector(config)
    }
    throw new Error('Invalid provider type')
  }
}
