import { AuthIdentity } from 'dcl-crypto'

import { ETHEREUM_NETWORK } from 'config'
import { Profile } from '../profiles/types'

export type RootSessionState = {
  session: SessionState
}

export type ExplorerIdentity = AuthIdentity & {
  address: string
  hasConnectedWeb3: boolean
}

export type SignUpData = {
  active: boolean
  tos?: boolean
  error?: AuthError | null
  errorMsg?: string | null
  profile: Partial<Profile>
}

export type SessionState = {
  initialized: boolean
  userId: string | undefined
  identity: ExplorerIdentity | undefined
  network: ETHEREUM_NETWORK | undefined
  signup: SignUpData
}

export enum AuthError {
  TOS_NOT_ACCEPTED = 'signup-tos-not-accepted',
  ACCOUNT_NOT_FOUND = 'signup-account-not-found',
  PROFILE_DOESNT_EXIST = 'signup-profile-doesnt-exist',
  PROFILE_ALREADY_EXISTS = 'signup-profile-already-exists'
}
