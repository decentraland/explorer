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
  TOS_NOT_ACCEPTED = 'TOS_NOT_ACCEPTED',
  ACCOUNT_NOT_FOUND = 'ACCOUNT_NOT_FOUND',
  PROFILE_DOESNT_EXIST = 'PROFILE_DOESNT_EXIST',
  PROFILE_ALREADY_EXISTS = 'PROFILE_ALREADY_EXISTS'
}
