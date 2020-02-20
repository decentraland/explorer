export type MetaConfiguration = {
  explorer: {
    minBuildNumber: number
  }
  servers: {
    added: string[]
    denied: string[]
  }
}

export type MetaState = {
  initialized: boolean
  config: Partial<MetaConfiguration>
}

export type RootMetaState = {
  meta: MetaState
}
