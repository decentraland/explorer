import { Profile } from './types'
import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { ETHEREUM_NETWORK } from 'config'

const LOCAL_PROFILES_KEY = 'dcl-local-profile'

export class LocalProfilesRepository {
  persist(address: string, network: ETHEREUM_NETWORK, profile: Profile) {
    // For now, we use local storage. BUT DON'T USE THIS KEY OUTSIDE BECAUSE THIS MIGHT CHANGE EVENTUALLY
    saveToLocalStorage(this.profileKey(address, network), profile)
  }

  remove(address: string, network: ETHEREUM_NETWORK) {
    localStorage.removeItem(this.profileKey(address, network))
  }

  get(address: string, network: ETHEREUM_NETWORK) {
    return getFromLocalStorage(this.profileKey(address, network))
  }

  private profileKey(address: string, network: ETHEREUM_NETWORK): string {
    return `${LOCAL_PROFILES_KEY}-${network}-${address}`
  }
}
