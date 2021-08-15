import { AuthIdentity } from 'dcl-crypto'

import { ETHEREUM_NETWORK } from 'config'
import { Profile } from '../profiles/types'
import { IEthereumProvider, LoginState } from '@dcl/kernel-interface'

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
  identity?: ExplorerIdentity
}

export type SessionState = {
  identity: ExplorerIdentity | undefined
  network: ETHEREUM_NETWORK | undefined
  loginState: LoginState | undefined
  isSignUp?: boolean
  signup: SignUpData
  isGuestLogin?: boolean
  provider?: IEthereumProvider
}

export type StoredSession = {
  identity: ExplorerIdentity
  isGuest?: boolean
}
