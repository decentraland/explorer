import { ETHEREUM_NETWORK, getTLD } from 'config'

declare var window: Window & {
  ethereum: any
}

// This method is similar to the one in web3.ts, but only returns a network if window.ethereum is defined
function getWeb3Network(): ETHEREUM_NETWORK | undefined {
  if (window.ethereum) {
    return window.ethereum.chainId === '0x1' ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN
  }
}

export function getNetworkFromTLD(tld: string = getTLD()): ETHEREUM_NETWORK | null {
  if (tld === 'zone') {
    return ETHEREUM_NETWORK.ROPSTEN
  }

  if (tld === 'today' || tld === 'org') {
    return ETHEREUM_NETWORK.MAINNET
  }

  // if localhost
  return null
}

export async function getNetworkFromTLDOrWeb3(): Promise<ETHEREUM_NETWORK> {
  const web3Network = getWeb3Network()

  if (web3Network) {
    return web3Network
  }

  return getNetworkFromTLD() || ETHEREUM_NETWORK.ROPSTEN
}
