import { Position } from './utils'
import { UserInformation, Package, ChatMessage, ProfileVersion, BusMessage, Pose } from './types'
import { Stats } from '../comms/debug'
import { ProfileForRenderer } from '../../decentraland-ecs/src/decentraland/Types'

export interface IWorldInstanceConnection {
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

  sendInitialMessage(userInfo: {
    userId?: string | undefined
    version?: number | undefined
    status?: string | undefined
    pose?: Pose | undefined
    profile?: ProfileForRenderer | undefined
  }): void
  sendProfileMessage(currentPosition: Position, userInfo: UserInformation): void
  sendPositionMessage(p: Position): void
  sendParcelUpdateMessage(currentPosition: Position, p: Position): void
  sendParcelSceneCommsMessage(cid: string, message: string): void
  sendChatMessage(currentPosition: Position, messageId: string, text: string): void

  // TODO - review if we want to change this to other interface - moliva - 19/12/2019
  updateSubscriptions(topics: string): void
}
