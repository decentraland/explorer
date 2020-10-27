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
  stage: string
  profile: Partial<Profile>
}

export enum LoginStage {
  LOADING = 'loading',
  SING_IN = 'signIn',
  SING_UP = 'signUp',
  CONNECT_ADVICE = 'connect_advice',
  SING_ADVICE = 'sign_advice',
  COMPLETED = 'completed'
}

export type SessionState = {
  initialized: boolean
  userId: string | undefined
  identity: ExplorerIdentity | undefined
  network: ETHEREUM_NETWORK | undefined
  loginStage: LoginStage | undefined
  tos: boolean
  showWalletPrompt: boolean
  signup: SignUpData
}
