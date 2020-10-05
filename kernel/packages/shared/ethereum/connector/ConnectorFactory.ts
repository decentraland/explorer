import { MetamaskConnector } from './MetamaskConnector'
import { ConnectorInterface } from './ConnectorInterface'
import { ProviderType } from '../ProviderType'

export class ConnectorFactory {
  create(type: ProviderType, network: string): ConnectorInterface {
    if (type === ProviderType.METAMASK) {
      return new MetamaskConnector()
    }
    throw new Error('Invalid provider type')
  }
}
