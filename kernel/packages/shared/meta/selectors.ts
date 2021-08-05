import { BannedUsers, CommsConfig, FeatureFlags, MessageOfTheDayConfig, RootMetaState } from './types'
import { Vector2Component } from 'atomicHelpers/landHelpers'
import { getCatalystNodesDefaultURL, VOICE_CHAT_DISABLED_FLAG } from 'config'

export const getAddedServers = (store: RootMetaState): string[] => {
  const { config } = store.meta

  if (!config || !config.servers || !config.servers.added) {
    return []
  }

  return config.servers.added
}

export const getContentWhitelist = (store: RootMetaState): string[] => {
  const { config } = store.meta

  if (!config || !config.servers || !config.servers.contentWhitelist) {
    return []
  }

  return config.servers.contentWhitelist
}

export const getMinCatalystVersion = (store: RootMetaState): string | undefined => {
  const { config } = store.meta

  return config.minCatalystVersion
}

export const isMetaConfigurationInitiazed = (store: RootMetaState): boolean => store.meta.initialized

export const getPois = (store: RootMetaState): Vector2Component[] => store.meta.config.world?.pois || []

export const getCommsConfig = (store: RootMetaState): CommsConfig => store.meta.config.comms ?? {}

export const getBannedUsers = (store: RootMetaState): BannedUsers => store.meta.config.bannedUsers ?? {}

export const isMOTDInitialized = (store: RootMetaState): boolean =>
  store.meta.config.world ? store.meta.config.world?.messageOfTheDayInit || false : false
export const getMessageOfTheDay = (store: RootMetaState): MessageOfTheDayConfig | null =>
  store.meta.config.world ? store.meta.config.world.messageOfTheDay || null : null

export const isVoiceChatEnabledFor = (store: RootMetaState, userId: string): boolean => !VOICE_CHAT_DISABLED_FLAG

export const isFeatureEnabled = (store: RootMetaState, featureName: FeatureFlags, ifNotSet: boolean): boolean => {
  const queryParamFlag = toUrlFlag(featureName)
  if (location.search.includes(`DISABLE_${queryParamFlag}`)) {
    return false
  } else if (location.search.includes(`ENABLE_${queryParamFlag}`)) {
    return true
  } else {
    const featureFlag = store?.meta.config?.featureFlags?.[`explorer-${featureName}`]
    return featureFlag ?? ifNotSet
  }
}

export const getSynapseUrl = (store: RootMetaState): string =>
  store.meta.config.synapseUrl ?? 'https://chat.decentraland.zone'

/** Convert camel case to upper snake case */
function toUrlFlag(key: string) {
  const result = key.replace(/([A-Z])/g, ' $1')
  return result.split(' ').join('_').toUpperCase()
}

export const getCatalystNodesEndpoint = (store: RootMetaState): string =>
  store.meta.config.servers?.catalystsNodesEndpoint ?? getCatalystNodesDefaultURL()
