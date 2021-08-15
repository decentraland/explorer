import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'

import { UserData } from 'shared/types'
import { calculateDisplayName } from 'shared/profiles/transformations/processServerProfile'
import { EnsureProfile } from 'shared/profiles/ProfileAsPromise'

import { ExposableAPI } from './ExposableAPI'
import { onLoginCompleted } from 'shared/session/sagas'

export interface IUserIdentity {
  /**
   * Return the Ethereum address of the user
   */
  getUserPublicKey(): Promise<string | null>

  /**
   * Return the user's data
   */
  getUserData(): Promise<UserData | null>
}

@registerAPI('Identity')
export class UserIdentity extends ExposableAPI implements IUserIdentity {
  @exposeMethod
  async getUserPublicKey(): Promise<string | null> {
    const { identity } = await onLoginCompleted()
    if (!identity || !identity.address) {
      debugger
    }
    return identity && identity.hasConnectedWeb3 ? identity.address : null
  }

  @exposeMethod
  async getUserData(): Promise<UserData | null> {
    const { identity } = await onLoginCompleted()

    if (!identity || !identity.address) {
      debugger
      return null
    }

    const profile = await EnsureProfile(identity?.address)

    return {
      displayName: calculateDisplayName(identity.address, profile),
      publicKey: identity.hasConnectedWeb3 ? identity.address : null,
      hasConnectedWeb3: !!identity.hasConnectedWeb3,
      userId: identity.address
    }
  }
}
