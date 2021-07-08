import { AuthIdentity } from 'dcl-crypto'

import { ETHEREUM_NETWORK } from 'config'
import { Profile } from '../profiles/types'
import { LoginStage } from '../../../../anti-corruption-layer/kernel-types'

export type RootSessionState = {
  session: SessionState
}

export type ExplorerIdentity = AuthIdentity & {
  address: string // contains the lowercased address that will be used for the userId
  rawAddress: string // contains the real ethereum address of the current user
  hasConnectedWeb3: boolean
}

export type SignUpData = {
  stage: string
  profile: Partial<Profile>
  userId?: string
  identity?: ExplorerIdentity
}

export type SessionState = {
  initialized: boolean
  userId: string | undefined
  identity: ExplorerIdentity | undefined
  network: ETHEREUM_NETWORK | undefined
  loginStage: LoginStage | undefined
  isSignUp?: boolean
  signup: SignUpData
}

export type StoredSession = {
  userId: string
  identity: ExplorerIdentity
}
