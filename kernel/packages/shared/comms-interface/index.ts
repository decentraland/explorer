import { Position } from './utils'
import { UserInformation, Package, ChatMessage, ProfileVersion, BusMessage } from './types'
import { Stats } from '../comms/debug'

export interface WorldInstanceConnection {
  stats: Stats | null

  // handlers
  sceneMessageHandler: (alias: string, data: Package<BusMessage>) => void
  chatHandler: (alias: string, data: Package<ChatMessage>) => void
  profileHandler: (alias: string, identity: string, data: Package<ProfileVersion>) => void
  positionHandler: (alias: string, data: Package<Position>) => void

  // TODO - review interface for the following members - moliva - 19/12/2019
  readonly isAuthenticated: boolean

  // TODO - review metrics API - moliva - 19/12/2019
  readonly ping: number
  printDebugInformation(): void

  close(): void

  sendInitialMessage(userInfo: Partial<UserInformation>): Promise<void>
  sendProfileMessage(currentPosition: Position, userInfo: UserInformation): Promise<void>
  sendPositionMessage(p: Position): Promise<void>
  sendParcelUpdateMessage(currentPosition: Position, p: Position): Promise<void>
  sendParcelSceneCommsMessage(cid: string, message: string): Promise<void>
  sendChatMessage(currentPosition: Position, messageId: string, text: string): Promise<void>

  // TODO - review if we want to change this to other interface - moliva - 19/12/2019
  updateSubscriptions(topics: string[]): Promise<void>
}
