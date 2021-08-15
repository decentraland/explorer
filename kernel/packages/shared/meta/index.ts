import { store } from 'shared/store/isolatedStore'
import { getMessageOfTheDay, isMetaConfigurationInitiazed, isMOTDInitialized } from './selectors'
import { MessageOfTheDayConfig } from './types'

export async function ensureMetaConfigurationInitialized(): Promise<void> {
  const initialized = isMetaConfigurationInitiazed(store.getState())
  if (initialized) {
    return Promise.resolve()
  }

  return new Promise<void>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const initialized = isMetaConfigurationInitiazed(store.getState())
      if (initialized) {
        unsubscribe()
        return resolve()
      }
    })
  })
}

export async function waitForMessageOfTheDay(): Promise<MessageOfTheDayConfig | null> {
  if (isMOTDInitialized(store.getState())) {
    return Promise.resolve(getMessageOfTheDay(store.getState()))
  }
  return new Promise<MessageOfTheDayConfig | null>((resolve) => {
    const unsubscribe = store.subscribe(() => {
      if (isMOTDInitialized(store.getState())) {
        unsubscribe()
        return resolve(getMessageOfTheDay(store.getState()))
      }
    })
  })
}
