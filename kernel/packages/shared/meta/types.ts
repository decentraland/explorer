export type MetaConfiguration = {
  explorer: {
    minBuildNumber: number
  }
  servers: {
    added: string[]
    denied: string[]
  }
}

export type MetaConfigurationState = {
  config: Partial<MetaConfiguration>
}

export type RootMetaState = {
  meta: MetaConfigurationState
}
