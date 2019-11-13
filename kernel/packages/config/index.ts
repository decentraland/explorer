declare var window: any

export const performanceConfigurations = [
  { antialiasing: true, downsampling: 0, shadows: true },
  { antialiasing: false, downsampling: 1, shadows: true },
  { antialiasing: false, downsampling: 1, shadows: false },
  { antialiasing: false, downsampling: 1, shadows: true },
  { antialiasing: false, downsampling: 2, shadows: false }
]

export const NETWORK_HZ = 10

export namespace interactionLimits {
  /**
   * click distance, this is the lenght of the ray/lens
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

  export const visibleRadius = 4
  export const secureRadius = 4

  export const maxX = 3000
  export const maxZ = 3000
  export const minX = -3000
  export const minZ = -3000

  export const maxParcelX = 150
  export const maxParcelZ = 150
  export const minParcelX = -150
  export const minParcelZ = -150

  export const minLandCoordinateX = -150
  export const minLandCoordinateY = -150
  export const maxLandCoordinateX = 150
  export const maxLandCoordinateY = 150
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
export const PREVIEW: boolean = !!(global as any).preview
export const EDITOR: boolean = !!(global as any).isEditor
export const WORLD_EXPLORER = !EDITOR && !PREVIEW

export const OPEN_AVATAR_EDITOR = location.search.indexOf('OPEN_AVATAR_EDITOR') !== -1 && WORLD_EXPLORER

export const STATIC_WORLD = location.search.indexOf('STATIC_WORLD') !== -1 || !!(global as any).staticWorld || EDITOR

// Development
export const ENABLE_WEB3 = location.search.indexOf('ENABLE_WEB3') !== -1 || !!(global as any).enableWeb3
export const ENV_OVERRIDE = location.search.indexOf('ENV') !== -1
export const USE_LOCAL_COMMS = location.search.indexOf('LOCAL_COMMS') !== -1 || PREVIEW
export const DEBUG = location.search.indexOf('DEBUG_MODE') !== -1 || !!(global as any).mocha || PREVIEW || EDITOR
export const DEBUG_ANALYTICS = location.search.indexOf('DEBUG_ANALYTICS') !== -1
export const DEBUG_MOBILE = location.search.indexOf('DEBUG_MOBILE') !== -1
export const DEBUG_MESSAGES = location.search.indexOf('DEBUG_MESSAGES') !== -1
export const DEBUG_WS_MESSAGES = location.search.indexOf('DEBUG_WS_MESSAGES') !== -1
export const DEBUG_REDUX = location.search.indexOf('DEBUG_REDUX') !== -1

export const DISABLE_AUTH = location.search.indexOf('DISABLE_AUTH') !== -1 || DEBUG
export const ENGINE_DEBUG_PANEL = location.search.indexOf('ENGINE_DEBUG_PANEL') !== -1
export const SCENE_DEBUG_PANEL = location.search.indexOf('SCENE_DEBUG_PANEL') !== -1 && !ENGINE_DEBUG_PANEL

export namespace commConfigurations {
  export const debug = true
  export const commRadius = 4

  export const peerTtlMs = 60000

  export const maxVisiblePeers = 25

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
}
export const loginConfig = {
  org: {
    domain: 'decentraland.auth0.com',
    client_id: 'yqFiSmQsxk3LK46JOIB4NJ3wK4HzZVxG'
  },
  today: {
    domain: 'dcl-stg.auth0.com',
    client_id: '0UB0I7w6QA3AgSvbXh9rGvDuhKrJV1C0'
  },
  zone: {
    domain: 'dcl-test.auth0.com',
    client_id: 'lTUEMnFpYb0aiUKeIRPbh7pBxKM6sccx'
  },
  audience: 'decentraland.org'
}

// take address from http://contracts.decentraland.org/addresses.json

export enum ETHEREUM_NETWORK {
  MAINNET = 'mainnet',
  ROPSTEN = 'ropsten'
}

export let decentralandConfigurations: any = {}
let contracts: any = null
let network: ETHEREUM_NETWORK | null = null

export function getTLD() {
  if (ENV_OVERRIDE) {
    return window.location.search.match(/ENV=(\w+)/)[1]
  }
  if (window) {
    return window.location.hostname.match(/(\w+)$/)[0]
  }
}

export const knownTLDs = ['zone', 'org', 'today']

function getDefaultTLD() {
  const TLD = getTLD()
  if (ENV_OVERRIDE) {
    return TLD
  }

  // web3 is now disabled by default
  if (!ENABLE_WEB3 && TLD === 'localhost') {
    return 'zone'
  }

  if (!TLD || !knownTLDs.includes(TLD)) {
    return network === ETHEREUM_NETWORK.ROPSTEN ? 'zone' : 'org'
  }

  return TLD
}

export function getExclusiveServer() {
  if (window.location.search.match(/WEARABLE_SERVER=\w+/)) {
    return window.location.search.match(/WEARABLE_SERVER=(\w+)/)[1]
  }
  return 'https://dcl-base-exclusive.now.sh/index.json'
}

export const ALL_WEARABLES = location.search.indexOf('ALL_WEARABLES') !== -1 && getDefaultTLD() !== 'org'

export function getLoginConfigurationForCurrentDomain() {
  let tld: 'org' | 'zone' | 'today' = getDefaultTLD()
  // Use `.zone` auth for any localhost or other edge case
  if ((tld as any) !== 'org' && (tld as any) !== 'zone' && (tld as any) !== 'today') {
    tld = 'zone'
  }
  return {
    clientId: loginConfig[tld].client_id,
    domain: loginConfig[tld].domain,
    redirectUri: window.location.origin + '/' + (ENV_OVERRIDE ? '?ENV=' + getTLD() : ''),
    audience: loginConfig.audience
  }
}

export const ENABLE_EMPTY_SCENES = !DEBUG || knownTLDs.includes(getTLD())

export function getServerConfigurations() {
  const TLDDefault = getDefaultTLD()
  return {
    auth: `https://auth.decentraland.${TLDDefault}/api/v1`,
    landApi: `https://api.decentraland.${TLDDefault}/v1`,
    content: `https://content.decentraland.${TLDDefault === 'today' ? 'org' : TLDDefault}`,
    worldInstanceUrl: `wss://world-comm.decentraland.${TLDDefault}/connect`,
    profile: `https://profile.decentraland.${TLDDefault}/api/v1`,
    wearablesApi: `https://wearable-api.decentraland.org/v1`,
    avatar: {
      snapshotStorage: `https://avatars-storage.decentraland.${TLDDefault}/`,
      server: `https://avatars-api.decentraland.${TLDDefault === 'zone' ? 'today' : TLDDefault}/`,
      catalog: 'https://dcl-base-avatars.now.sh/index.json',
      exclusiveCatalog: getExclusiveServer(),
      contents: `https://s3.amazonaws.com/content-service.decentraland.org/`,
      presets: `https://avatars-storage.decentraland.org/mobile-avatars`
    },
    darApi:
      TLDDefault === 'zone' || TLDDefault === 'today'
        ? 'https://schema-api-v2.now.sh/dar'
        : 'https://schema.decentraland.org/dar'
  }
}

export async function setNetwork(net: ETHEREUM_NETWORK) {
  try {
    const response = await fetch('https://contracts.decentraland.org/addresses.json')
    const json = await response.json()

    network = net
    contracts = json[net]

    decentralandConfigurations = {
      contractAddress: contracts.LANDProxy,
      contracts: {
        serviceLocator: contracts.ServiceLocator
      },
      paymentTokens: {
        MANA: contracts.MANAToken
      }
    }
  } catch (e) {
    // Could not fetch addresses. You might be offline. Setting sensitive defaults for contract addresses...

    network = net
    contracts = {}

    decentralandConfigurations = {
      contractAddress: '',
      contracts: {
        serviceLocator: ''
      },
      paymentTokens: {
        MANA: ''
      }
    }
  }
}

export namespace ethereumConfigurations {
  export const mainnet = {
    wss: 'wss://mainnet.infura.io/ws',
    http: 'https://mainnet.infura.io/',
    etherscan: 'https://etherscan.io'
  }
  export const ropsten = {
    wss: 'wss://ropsten.infura.io/ws',
    http: 'https://ropsten.infura.io/',
    etherscan: 'https://ropsten.etherscan.io'
  }
}

export const isRunningTest: boolean = (global as any)['isRunningTests'] === true
