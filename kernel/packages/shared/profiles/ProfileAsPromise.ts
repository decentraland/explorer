import { RootState, StoreContainer } from '../store/rootTypes'
import { Store } from 'redux'
import { getProfile, getProfileStatusAndData } from './selectors'
import { profileRequest } from './actions'
import { Profile } from './types'

declare const globalThis: StoreContainer

export function ProfileAsPromise(userId: string, version?: number): Promise<Profile> {
  const store: Store<RootState> = globalThis.globalStore

  const existingProfile = getProfile(store.getState(), userId)
  const existingProfileWithCorrectVersion = existingProfile && (!version || existingProfile.version >= version)
  if (existingProfile && existingProfileWithCorrectVersion) {
    return Promise.resolve(existingProfile)
  }
  return new Promise((resolve, reject) => {
    const unsubscribe = store.subscribe(() => {
      const [status, data] = getProfileStatusAndData(store.getState(), userId)

      if (status === 'error') {
        unsubscribe()
        return reject(data)
      }

      const profile = getProfile(store.getState(), userId)
      if (profile) {
        unsubscribe()
        return resolve(profile)
      }
    })
    store.dispatch(profileRequest(userId))
  })
}

export function EnsureProfile(userId: string, version?: number): Promise<Profile> {
  const store: Store<RootState> = globalThis.globalStore
  const existingProfile = getProfile(store.getState(), userId)
  const existingProfileWithCorrectVersion = existingProfile && (!version || existingProfile.version >= version)
  if (existingProfile && existingProfileWithCorrectVersion) {
    return Promise.resolve(existingProfile)
  }
  return new Promise((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const profile = getProfile(store.getState(), userId)
      if (profile) {
        unsubscribe()
        return resolve(profile)
      }
    })
  })
}
