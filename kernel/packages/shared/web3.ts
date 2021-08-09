import { ETHEREUM_NETWORK, ethereumConfigurations } from 'config'
import { Address } from 'web3x/address'
import { Catalyst } from './dao/contracts/Catalyst'
import { defaultLogger } from './logger'
import { CatalystNode, GraphResponse } from './types'
import { retry } from '../atomicHelpers/retry'
import { Fetcher } from 'dcl-catalyst-commons'
import { Eth } from 'web3x/eth'
import { requestManager } from './ethereum/provider'

declare var window: Window & {
  ethereum: any
}

export async function getAppNetwork(): Promise<ETHEREUM_NETWORK> {
  const web3Network = await requestManager.net_version()
  const web3net = parseInt(web3Network, 10) === 1 ? ETHEREUM_NETWORK.MAINNET : ETHEREUM_NETWORK.ROPSTEN
  return web3net
}

export async function fetchCatalystNodesFromDAO(): Promise<CatalystNode[]> {
  if (!requestManager.provider) {
    debugger
    throw new Error('requestManager.provider not set')
  }

  const net = await getAppNetwork()

  debugger
  const catalystContractAddress = Address.fromString(ethereumConfigurations[net].CatalystProxy)
  const eth = new Eth(requestManager.provider)

  const contract = new Catalyst(eth, catalystContractAddress)

  const count = Number.parseInt(await retry(() => contract.methods.catalystCount().call()), 10)

  const nodes = []
  for (let i = 0; i < count; ++i) {
    const ids = await retry(() => contract.methods.catalystIds(i).call())
    const node = await retry(() => contract.methods.catalystById(ids).call())

    if (node.domain.startsWith('http://')) {
      defaultLogger.warn(`Catalyst node domain using http protocol, skipping ${node.domain}`)
      continue
    }

    if (!node.domain.startsWith('https://')) {
      node.domain = 'https://' + node.domain
    }

    // trim url in case it starts/ends with a blank
    node.domain = node.domain.trim()

    nodes.push(node)
  }

  return nodes
}

export async function fetchOwnedENS(theGraphBaseUrl: string, ethAddress: string): Promise<string[]> {
  const query = `
query GetNameByBeneficiary($beneficiary: String) {
  nfts(where: { owner: $beneficiary, category: ens }) {
    ens {
      labelHash
      beneficiary
      caller
      subdomain
      createdAt
    }
  }
}`

  const variables = { beneficiary: ethAddress.toLowerCase() }

  try {
    const jsonResponse: GraphResponse = await queryGraph(theGraphBaseUrl, query, variables)
    return jsonResponse.nfts.map((nft) => nft.ens.subdomain)
  } catch (e) {
    // do nothing
  }
  return []
}

export async function fetchENSOwner(url: string, name: string) {
  const query = `
    query GetOwner($name: String!) {
      nfts(first: 1, where: { searchText: $name, category: ens  }) {
        owner{
          address
        }
      }
    }`

  const variables = { name: name.toLowerCase() }

  try {
    const resp = await queryGraph(url, query, variables)
    return resp.nfts.length === 1 ? (resp.nfts[0].owner.address as string) : null
  } catch (error) {
    defaultLogger.error(`Error querying graph`, error)
    throw error
  }
}

/**
 * Fetch owners of ENS (names) that contains string "name"
 * @param url query url
 * @param name string to query
 * @param maxResults max results expected (The Graph support up to 1000)
 */
export async function fetchENSOwnersContains(url: string, name: string, maxResults: number): Promise<string[]> {
  const query = `
    query GetOwner($name: String!, $maxResults: Int!) {
      nfts(first: $maxResults, where: { searchText_contains: $name, category: ens }) {
        owner{
          address
        }
      }
    }`

  const variables = { name: name.toLowerCase(), maxResults }

  try {
    const response = await queryGraph(url, query, variables)
    return response.nfts.map((nft: any) => nft.owner.address as string)
  } catch (error) {
    defaultLogger.error(`Error querying graph`, error)
    throw error
  }
}

async function queryGraph(url: string, query: string, variables: any, totalAttempts: number = 5) {
  const fetcher = new Fetcher()
  return fetcher.queryGraph(url, query, variables, { attempts: totalAttempts })
}

/**
 * Register to any change in the configuration of the wallet to reload the app and avoid wallet changes in-game.
 */
export function registerProviderNetChanges() {
  if (window.ethereum && typeof window.ethereum.on === 'function') {
    window.ethereum.on('chainChanged', () => location.reload())
  }
}
