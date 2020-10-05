import { Vector2Component } from 'atomicHelpers/landHelpers'
import future, { IFuture } from 'fp-future'

export let USE_UNITY_INDEXED_DB_CACHE: IFuture<boolean> = future()

export type MetaConfiguration = {
  explorer: {
    minBuildNumber: number
    useUnityIndexedDbCache: boolean
  }
  servers: {
    added: string[]
    denied: string[]
    contentWhitelist: string[]
  }
  world: {
    pois: Vector2Component[]
    currentWorldEvent: WorldEventState 
  }
  comms: CommsConfig
}

export enum WorldEventState {
  DEFAULT = 0,
  HALLOWEEN = 1,
}

export type MetaState = {
  initialized: boolean
  config: Partial<MetaConfiguration>
}

export type RootMetaState = {
  meta: MetaState
}

export type CommsConfig = {
  targetConnections?: number
  maxConnections?: number
  relaySuspensionDisabled?: boolean
  relaySuspensionInterval?: number
  relaySuspensionDuration?: number
}
