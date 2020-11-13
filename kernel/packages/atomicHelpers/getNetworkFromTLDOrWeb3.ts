import { getNetworkFromTLD, getDefaultTLD, ETHEREUM_NETWORK } from 'config'
import { getWeb3Network } from 'shared/web3'
export function getNetworkFromTLDOrWeb3(): ETHEREUM_NETWORK {
  const tldNetwork = getNetworkFromTLD()

  if (tldNetwork) {
    return tldNetwork
  }

  const web3Network = getWeb3Network()

  if (web3Network) {
    return web3Network
  } else {
    return getNetworkFromTLD(getDefaultTLD())!
  }
}
