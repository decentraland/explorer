export type Layer = {
  name: string
  usersCount: number
  maxUsers: number
  usersParcels?: [number, number][]
}

export enum ServerConnectionStatus {
  OK,
  UNREACHABLE
}

export type CatalystStatus = {
  name: string
  version: string
  layers?: Layer[]
  usersCount?: number
  env: {
    catalystVersion: string
  }
}

type BaseCandidate = {
  domain: string
  catalystName: string
  elapsed: number
  score: number
  status: ServerConnectionStatus
  lighthouseVersion: string
  catalystVersion: string
}

export type LayerBasedCandidate = {
  type: 'layer-based'
  layer: Layer
} & BaseCandidate

export type IslandsBasedCandidate = {
  type: 'islands-based'
  usersCount: number
} & BaseCandidate

export type Candidate = LayerBasedCandidate | IslandsBasedCandidate

export type LayerUserInfo = {
  userId: string
  peerId: string
  protocolVersion: number
  parcel?: [number, number]
}

export type Realm = {
  domain: string
  catalystName: string
  layer?: string
  lighthouseVersion: string
}

export type DaoState = {
  initialized: boolean
  candidatesFetched: boolean
  fetchContentServer: string
  catalystServer: string
  updateContentServer: string
  commsServer: string
  resizeService: string
  hotScenesService: string
  exploreRealmsService: string
  poiService: string
  realm: Realm | undefined
  candidates: Candidate[]
  contentWhitelist: Candidate[]
  addedCandidates: Candidate[]
  commsStatus: CommsStatus
}

export type RootDaoState = {
  dao: DaoState
}

export type CommsState =
  | 'initial'
  | 'connecting'
  | 'connected'
  | 'error'
  | 'realm-full'
  | 'reconnection-error'
  | 'id-taken'

export type CommsStatus = {
  status: CommsState
  connectedPeers: number
}

export type PingResult = {
  elapsed?: number
  status?: ServerConnectionStatus
  result?: CatalystStatus
}

export enum HealthStatus {
  HEALTHY = 'Healthy',
  UNHEALTHY = 'Unhealthy',
  DOWN = 'Down'
}
