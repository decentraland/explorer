import { CommsConfig, MessageOfTheDayConfig, RootMetaState } from './types'
import { Vector2Component } from 'atomicHelpers/landHelpers'
import { VOICE_CHAT_DISABLED_FLAG, VOICE_CHAT_ENABLED_FLAG } from 'config'

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

export const isMetaConfigurationInitiazed = (store: RootMetaState): boolean => store.meta.initialized

export const getPois = (store: RootMetaState): Vector2Component[] => store.meta.config.world?.pois || []

export const getCommsConfig = (store: RootMetaState): CommsConfig => store.meta.config.comms ?? {}

export const isMOTDInitialized = (store: RootMetaState): boolean =>
  store.meta.config.world ? store.meta.config.world?.messageOfTheDayInit || false : false
export const getMessageOfTheDay = (store: RootMetaState): MessageOfTheDayConfig | null =>
  store.meta.config.world ? store.meta.config.world.messageOfTheDay || null : null

export const isVoiceChatEnabled = (store: RootMetaState): boolean =>
  (!!getCommsConfig(store).voiceChatEnabled && !VOICE_CHAT_DISABLED_FLAG) || VOICE_CHAT_ENABLED_FLAG

export const getVoiceChatWhitelist = (store: RootMetaState): string[] => getCommsConfig(store).voiceChatAllowlist ?? []

export const isVoiceChatEnabledFor = (store: RootMetaState, userId: string): boolean =>
  isVoiceChatEnabled(store) || getVoiceChatWhitelist(store).includes(userId)
