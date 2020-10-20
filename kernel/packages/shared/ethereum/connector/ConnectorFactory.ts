import { MetamaskConnector } from './MetamaskConnector'
import { ConnectorInterface } from './ConnectorInterface'
import { ProviderType } from '../ProviderType'
import { GuestConnector } from './GuestConnector'
import { ETHEREUM_NETWORK, ethereumConfigurations } from '../../../config'

export class ConnectorFactory {
  create(type: ProviderType, network: ETHEREUM_NETWORK): ConnectorInterface {
    if (type === ProviderType.METAMASK) {
      return new MetamaskConnector()
    }
    if (type === ProviderType.GUEST) {
      return new GuestConnector(ethereumConfigurations[network].wss)
    }
    throw new Error('Invalid provider type')
  }
}
