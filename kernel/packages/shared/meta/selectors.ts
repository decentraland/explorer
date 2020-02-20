import { RootMetaState } from './types'

export const getAddedServers = (store: RootMetaState): string[] => {
  const { config } = store.meta

  if (!config || !config.servers || !config.servers.added) {
    return []
  }

  return config.servers.added
}

export const isMetaConfigurationInitiazed = (store: RootMetaState): boolean => store.meta.initialized
