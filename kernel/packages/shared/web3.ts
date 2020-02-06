import { ETHEREUM_NETWORK, getTLD, getServerConfigurations } from '../config'

import { getUserAccount, getNetwork } from './ethereum/EthereumService'
import { awaitWeb3Approval } from './ethereum/provider'
import { queueTrackingEvent } from './analytics'
import { defaultLogger } from './logger'

import { Address } from 'web3x/address'
import { Eth } from 'web3x/eth'
import { Catalyst } from './dao/contracts/Catalyst'

async function getAddress(): Promise<string | undefined> {
  try {
    await awaitWeb3Approval()
    return await getUserAccount()
  } catch (e) {
    defaultLogger.info(e)
  }
}

export function getNetworkFromTLD(): ETHEREUM_NETWORK | null {
  const tld = getTLD()
  if (tld === 'zone') {
    return ETHEREUM_NETWORK.ROPSTEN
  }

  if (tld === 'today' || tld === 'org') {
    return ETHEREUM_NETWORK.MAINNET
  }

  return null
}

export async function getAppNetwork(): Promise<ETHEREUM_NETWORK> {
  const web3Network = await getNetwork()
  const web3net = web3Network === '1' ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN
  // TLD environment have priority
  const net = getNetworkFromTLD() || web3net

  if (web3net && net !== web3net) {
    // TODO @fmiras show an HTML error if web3 networks differs from domain network and do not load client at all
    defaultLogger.error(`Switch to network ${net}`)
  }

  defaultLogger.info('Using ETH network: ', net)
  return net
}

export async function initWeb3(): Promise<void> {
  const address = await getAddress()

  if (address) {
    defaultLogger.log(`Identifying address ${address}`)
    queueTrackingEvent('Use web3 address', { address })
  }
}

export async function fetchCatalystNodes() {
  const contractAddress = Address.fromString(getServerConfigurations().dao)
  const eth = Eth.fromCurrentProvider()

  if (!eth) {
    throw new Error('Ethereum provider not set!')
  }

  const contract = new Catalyst(eth, contractAddress)

  const count = Number.parseInt(await contract.methods.catalystCount().call(), 10)

  const nodes = []
  for (let i = 0; i < count; ++i) {
    const ids = await contract.methods.catalystIds(i).call()
    const node = await contract.methods.catalystById(ids).call()

    nodes.push(node)
  }

  return nodes
}
