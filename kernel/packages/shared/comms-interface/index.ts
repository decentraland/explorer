import { Position } from './utils'
import { UserInformation } from './types'

export interface IWorldInstanceConnection {
  // TODO - review interface for the following members - moliva - 19/12/2019
  readonly isAuthenticated: boolean

  // TODO - review metrics API - moliva - 19/12/2019
  readonly ping: number
  printDebugInformation(): void

  close(): void

  sendProfileMessage(currentPosition: Position, userInfo: UserInformation): void
  sendPositionMessage(p: Position): void
  sendParcelUpdateMessage(currentPosition: Position, p: Position): void
  sendParcelSceneCommsMessage(cid: string, message: string): void
  sendChatMessage(currentPosition: Position, messageId: string, text: string): void

  // TODO - review if we want to change this to other interface - moliva - 19/12/2019
  updateSubscriptions(topics: string): void
}
