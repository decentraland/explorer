import * as contractInfo from '@dcl/urn-resolver/dist/contracts'
import * as queryString from 'query-string'
import { getWorld } from '@dcl/schemas'
import { store } from 'shared/store/isolatedStore'

export const NETWORK_HZ = 10

export namespace interactionLimits {
  /**
   * click distance, this is the length of the ray/lens
   */
  export const clickDistance = 10
}

export namespace parcelLimits {
  // Maximum numbers for parcelScenes to prevent performance problems
  // Note that more limitations may be added to this with time
  // And we may also measure individual parcelScene performance (as
  // in webgl draw time) and disable parcelScenes based on that too,
  // Performance / anti-ddos work is a fluid area.

  // number of entities
  export const entities = 200

  // Number of faces (per parcel)
  export const triangles = 10000
  export const bodies = 300
  export const textures = 10
  export const materials = 20
  export const height = 20
  export const geometries = 200

  export const parcelSize = 16 /* meters */
  export const halfParcelSize = parcelSize / 2 /* meters */
  export const centimeter = 0.01

  export let visibleRadius = 4

  /**
   * @deprecated. This is still used to calculate a position hash, but shouln't be used for anything else.
   */
  export const maxParcelX = 150
  /** @deprecated */
  export const maxParcelZ = 150
  /** @deprecated */
  export const minParcelX = -150
  /** @deprecated */
  export const minParcelZ = -150

  export const descriptiveValidWorldRanges = getWorld()
    .validWorldRanges.map(
      (range) => `(X from ${range.xMin} to ${range.xMax}, and Y from ${range.yMin} to ${range.yMax})`
    )
    .join(' or ')
}
export namespace playerConfigurations {
  export const gravity = -0.2
  export const height = 1.6
  export const handFromBodyDistance = 0.5
  // The player speed
  export const speed = 2
  export const runningSpeed = 8
  // The player inertia
  export const inertia = 0.01
  // The mouse sensibility (lower is most sensible)
  export const angularSensibility = 500
}

export namespace visualConfigurations {
  export const fieldOfView = 75
  export const farDistance = parcelLimits.visibleRadius * parcelLimits.parcelSize

  export const near = 0.08
  export const far = farDistance
}

// Entry points
export const PREVIEW: boolean = !!(globalThis as any).preview
export const EDITOR: boolean = !!(globalThis as any).isEditor
export const WORLD_EXPLORER = !EDITOR && !PREVIEW

export const OPEN_AVATAR_EDITOR = location.search.includes('OPEN_AVATAR_EDITOR') && WORLD_EXPLORER

export const STATIC_WORLD = location.search.includes('STATIC_WORLD') || !!(globalThis as any).staticWorld || EDITOR

// Development
export const ENV_OVERRIDE = location.search.includes('ENV')
export const GIF_WORKERS = location.search.includes('GIF_WORKERS')

const qs = queryString.parse(location.search)

function ensureSingleString(value: string | string[] | null): string | null {
  if (!value) return null
  if (typeof value === 'string') return value
  return value[0]
}

// Comms
export const USE_LOCAL_COMMS = location.search.includes('LOCAL_COMMS') || PREVIEW
export const COMMS = USE_LOCAL_COMMS ? 'v1-local' : qs.COMMS ? ensureSingleString(qs.COMMS)! : 'v2-p2p' // by default
export const COMMS_PROFILE_TIMEOUT = 10000

export const UPDATE_CONTENT_SERVICE = ensureSingleString(qs.UPDATE_CONTENT_SERVICE)
export const FETCH_CONTENT_SERVICE = ensureSingleString(qs.FETCH_CONTENT_SERVICE)
export const COMMS_SERVICE = ensureSingleString(qs.COMMS_SERVICE)
export const RESIZE_SERVICE = ensureSingleString(qs.RESIZE_SERVICE)
export const HOTSCENES_SERVICE = ensureSingleString(qs.HOTSCENES_SERVICE)
export const POI_SERVICE = ensureSingleString(qs.POI_SERVICE)
export const REALM = ensureSingleString(qs.realm)
export const PREFERED_ISLAND = ensureSingleString(qs.island)

export const VOICE_CHAT_DISABLED_FLAG = location.search.includes('VOICE_CHAT_DISABLED')

export const AUTO_CHANGE_REALM = location.search.includes('AUTO_CHANGE_REALM')

export const LOS = ensureSingleString(qs.LOS)

export const DEBUG = location.search.includes('DEBUG_MODE') || !!(global as any).mocha || PREVIEW || EDITOR
export const DEBUG_ANALYTICS = location.search.includes('DEBUG_ANALYTICS')
export const DEBUG_MOBILE = location.search.includes('DEBUG_MOBILE')
export const DEBUG_MESSAGES = location.search.includes('DEBUG_MESSAGES')
export const DEBUG_MESSAGES_QUEUE_PERF = location.search.includes('DEBUG_MESSAGES_QUEUE_PERF')
export const DEBUG_WS_MESSAGES = location.search.includes('DEBUG_WS_MESSAGES')
export const DEBUG_REDUX = location.search.includes('DEBUG_REDUX')
export const DEBUG_LOGIN = location.search.includes('DEBUG_LOGIN')
export const DEBUG_PM = location.search.includes('DEBUG_PM')
export const DEBUG_SCENE_LOG = DEBUG || location.search.includes('DEBUG_SCENE_LOG')

export const RESET_TUTORIAL = location.search.includes('RESET_TUTORIAL')

export const ENGINE_DEBUG_PANEL = location.search.includes('ENGINE_DEBUG_PANEL')
export const SCENE_DEBUG_PANEL = location.search.includes('SCENE_DEBUG_PANEL') && !ENGINE_DEBUG_PANEL
export const SHOW_FPS_COUNTER = location.search.includes('SHOW_FPS_COUNTER') || DEBUG
export const HAS_INITIAL_POSITION_MARK = location.search.includes('position')
export const WSS_ENABLED = !!ensureSingleString(qs.ws)
export const FORCE_SEND_MESSAGE = location.search.includes('FORCE_SEND_MESSAGE')

export const NO_ASSET_BUNDLES = location.search.includes('NO_ASSET_BUNDLES')
export const ASSET_BUNDLES_DOMAIN = ensureSingleString(qs.ASSET_BUNDLES_DOMAIN)

export const PIN_CATALYST = PREVIEW
  ? location.origin
  : typeof qs.CATALYST === 'string'
  ? addHttpsIfNoProtocolIsSet(qs.CATALYST)
  : undefined

export const FORCE_RENDERING_STYLE = ensureSingleString(qs.FORCE_RENDERING_STYLE) as any

const META_CONFIG_URL = ensureSingleString(qs.META_CONFIG_URL)

export namespace commConfigurations {
  export const debug = true
  export const commRadius = 4

  export const sendAnalytics = true

  export const peerTtlMs = 60000

  export const maxVisiblePeers = typeof qs.MAX_VISIBLE_PEERS === 'string' ? parseInt(qs.MAX_VISIBLE_PEERS, 10) : 25

  export const autoChangeRealmInterval =
    typeof qs.AUTO_CHANGE_INTERVAL === 'string' ? parseInt(qs.AUTO_CHANGE_INTERVAL, 10) * 1000 : 40000

  export const iceServers = [
    {
      urls: 'stun:stun.l.google.com:19302'
    },
    {
      urls: 'stun:stun2.l.google.com:19302'
    },
    {
      urls: 'stun:stun3.l.google.com:19302'
    },
    {
      urls: 'stun:stun4.l.google.com:19302'
    },
    {
      urls: 'turn:stun.decentraland.org:3478',
      credential: 'passworddcl',
      username: 'usernamedcl'
    }
  ]

  export const voiceChatUseHRTF = location.search.includes('VOICE_CHAT_USE_HRTF')
}

// take address from http://contracts.decentraland.org/addresses.json

export enum ETHEREUM_NETWORK {
  MAINNET = 'mainnet',
  ROPSTEN = 'ropsten'
}

export const knownTLDs = ['zone', 'org', 'today']

// return one of org zone today
export function getTLD() {
  if (ENV_OVERRIDE) {
    return location.search.match(/ENV=(\w+)/)![1]
  }
  const previsionalTld = location.hostname.match(/(\w+)$/)![0]
  if (knownTLDs.includes(previsionalTld)) return previsionalTld
  return 'org'
}

export const WITH_FIXED_COLLECTIONS = (qs.WITH_COLLECTIONS && ensureSingleString(qs.WITH_COLLECTIONS)) || ''
export const ENABLE_EMPTY_SCENES = !DEBUG || knownTLDs.includes(getTLD())

export function getAssetBundlesBaseUrl(network: ETHEREUM_NETWORK): string {
  const state = store.getState()
  return (
    ASSET_BUNDLES_DOMAIN || state.meta.config.explorer?.assetBundlesFetchUrl || getDefaultAssetBundlesBaseUrl(network)
  )
}

function getDefaultAssetBundlesBaseUrl(network: ETHEREUM_NETWORK): string {
  const tld = network == ETHEREUM_NETWORK.MAINNET ? 'org' : 'zone'
  return `https://content-assets-as-bundle.decentraland.${tld}`
}

export function getServerConfigurations(network: ETHEREUM_NETWORK) {
  const tld = network == ETHEREUM_NETWORK.MAINNET ? 'org' : 'zone'

  const metaConfigBaseUrl = META_CONFIG_URL || `https://config.decentraland.${tld}/explorer.json`
  const metaFeatureFlagsBaseUrl = `https://feature-flags.decentraland.${tld}/explorer.json`

  const questsUrl =
    ensureSingleString(qs.QUESTS_SERVER_URL) ?? `https://quests-api.decentraland.${network ? 'org' : 'io'}`

  return {
    explorerConfiguration: `${metaConfigBaseUrl}?t=${new Date().getTime()}`,
    explorerFeatureFlags: `${metaFeatureFlagsBaseUrl}?t=${new Date().getTime()}`,
    questsUrl,
    fallbackResizeServiceUrl: `${PIN_CATALYST ?? 'https://peer.decentraland.' + tld}/lambdas/images`
  }
}

function assertValue<T>(val: T | undefined | null): T {
  if (!val) throw new Error('Value is missing')
  return val
}

export namespace ethereumConfigurations {
  export const mainnet = {
    wss: 'wss://mainnet.infura.io/ws/v3/074a68d50a7c4e6cb46aec204a50cbf0',
    http: 'https://mainnet.infura.io/v3/074a68d50a7c4e6cb46aec204a50cbf0/',
    etherscan: 'https://etherscan.io',
    names: 'https://api.thegraph.com/subgraphs/name/decentraland/marketplace',

    // contracts
    LANDProxy: assertValue(contractInfo.mainnet.LANDProxy),
    EstateProxy: assertValue(contractInfo.mainnet.EstateProxy),
    CatalystProxy: assertValue(contractInfo.mainnet.CatalystProxy),
    MANAToken: assertValue(contractInfo.mainnet.MANAToken)
  }
  export const ropsten = {
    wss: 'wss://ropsten.infura.io/ws/v3/074a68d50a7c4e6cb46aec204a50cbf0',
    http: 'https://ropsten.infura.io/v3/074a68d50a7c4e6cb46aec204a50cbf0/',
    etherscan: 'https://ropsten.etherscan.io',
    names: 'https://api.thegraph.com/subgraphs/name/decentraland/marketplace-ropsten',

    // contracts
    LANDProxy: assertValue(contractInfo.ropsten.LANDProxy),
    EstateProxy: assertValue(contractInfo.ropsten.EstateProxy),
    CatalystProxy: assertValue(contractInfo.ropsten.CatalystProxy || contractInfo.ropsten.Catalyst),
    MANAToken: assertValue(contractInfo.ropsten.MANAToken)
  }
}

export const isRunningTest: boolean = (global as any)['isRunningTests'] === true

export const genericAvatarSnapshots: Record<string, string> = {
  face: '/images/avatar_snapshot_default.png',
  body: '/images/image_not_found.png',
  face256: '/images/avatar_snapshot_default256.png',
  face128: '/images/avatar_snapshot_default128.png'
}

export function getCatalystNodesDefaultURL() {
  return `https://peer-lb.decentraland.${getTLD()}/lambdas/contracts/servers`
}

function addHttpsIfNoProtocolIsSet(domain: string): string {
  if (!domain.startsWith('http')) {
    return `https://${domain}`
  }
  return domain
}
