import { Position } from './utils'
import { UserInformation, Package, ChatMessage, ProfileVersion, BusMessage } from './types'
import { Stats } from '../debug'
import { Realm } from 'shared/dao/types'

export interface WorldInstanceConnection {
  stats: Stats | null

  // handlers
  sceneMessageHandler: (alias: string, data: Package<BusMessage>) => void
  chatHandler: (alias: string, data: Package<ChatMessage>) => void
  profileHandler: (alias: string, identity: string, data: Package<ProfileVersion>) => void
  positionHandler: (alias: string, data: Package<Position>) => void

  readonly isAuthenticated: boolean

  // TODO - review metrics API - moliva - 19/12/2019
  readonly ping: number
  printDebugInformation(): void
  analyticsData(): any

  close(): void

  sendInitialMessage(userInfo: Partial<UserInformation>): Promise<void>
  sendProfileMessage(currentPosition: Position, userInfo: UserInformation): Promise<void>
  sendPositionMessage(p: Position): Promise<void>
  sendParcelUpdateMessage(currentPosition: Position, p: Position): Promise<void>
  sendParcelSceneCommsMessage(cid: string, message: string): Promise<void>
  sendChatMessage(currentPosition: Position, messageId: string, text: string): Promise<void>

  updateSubscriptions(topics: string[]): Promise<void>

  changeRealm(realm: Realm, url: string): Promise<void>

  connectPeer(): Promise<void>
}
