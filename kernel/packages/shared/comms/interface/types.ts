import { ProfileForRenderer } from 'decentraland-ecs/src'
import { AuthIdentity } from 'dcl-crypto'

export enum AvatarMessageType {
  // Networking related messages
  USER_DATA = 'USER_DATA',
  USER_POSE = 'USER_POSE',
  USER_VISIBLE = 'USER_VISIBLE',
  USER_EXPRESSION = 'USER_EXPRESSION',
  USER_REMOVED = 'USER_REMOVED',
  SET_LOCAL_UUID = 'SET_LOCAL_UUID',

  // Actions related messages
  USER_MUTED = 'USER_MUTED',
  USER_UNMUTED = 'USER_UNMUTED',
  USER_BLOCKED = 'USER_BLOCKED',
  USER_UNBLOCKED = 'USER_UNBLOCKED',

  ADD_FRIEND = 'ADD_FRIEND',
  SHOW_WINDOW = 'SHOW_WINDOW'
}

export type ReceiveUserExpressionMessage = {
  type: AvatarMessageType.USER_EXPRESSION
  uuid: string
  expressionId: string
  timestamp: number
}

export type ReceiveUserDataMessage = {
  type: AvatarMessageType.USER_DATA
  uuid: string
  data: Partial<UserInformation>
}

export type ReceiveUserVisibleMessage = {
  type: AvatarMessageType.USER_VISIBLE
  uuid: string
  visible: boolean
}

export type ReceiveUserPoseMessage = {
  type: AvatarMessageType.USER_POSE
  uuid: string
  pose: Pose
}

export type UserRemovedMessage = {
  type: AvatarMessageType.USER_REMOVED
  uuid: string
}

export type UserMessage = {
  type:
    | AvatarMessageType.SET_LOCAL_UUID
    | AvatarMessageType.USER_BLOCKED
    | AvatarMessageType.USER_UNBLOCKED
    | AvatarMessageType.USER_MUTED
    | AvatarMessageType.USER_UNMUTED
    | AvatarMessageType.SHOW_WINDOW
  uuid: string
}

export type AvatarMessage =
  | ReceiveUserDataMessage
  | ReceiveUserPoseMessage
  | ReceiveUserVisibleMessage
  | ReceiveUserExpressionMessage
  | UserRemovedMessage
  | UserMessage

export type UUID = string

/**
 * This type contains information about the peers, the AvatarEntity must accept this whole object in setAttributes(obj).
 */
export type PeerInformation = {
  /**
   * Unique peer ID
   */
  uuid: UUID

  flags: {
    muted?: boolean
  }

  user?: UserInformation
}

export type UserInformation = {
  userId?: string
  version?: number
  status?: string
  pose?: Pose
  expression?: AvatarExpression
  profile?: ProfileForRenderer
  identity?: AuthIdentity
}

export type AvatarExpression = {
  expressionType?: string
  expressionTimestamp?: number
}

// The order is [X,Y,Z,Qx,Qy,Qz,Qw]
export type Pose = [number, number, number, number, number, number, number]

export type PoseInformation = {
  v: Pose
}

export type PackageType = 'profile' | 'chat' | 'position'

export type Package<T> = {
  type: PackageType
  time: number
  data: T
}

export type ProfileVersion = {
  version: string
  user: string // TODO - to remove with new login flow - moliva - 22/12/2019
}

export type ChatMessage = {
  id: string
  text: string
}

export type BusMessage = ChatMessage

export class IdTakenError extends Error {
  constructor(message: string) {
    super(message)
  }
}
export class ConnectionEstablishmentError extends Error {
  constructor(message: string) {
    super(message)
  }
}
export class UnknownCommsModeError extends Error {
  constructor(message: string) {
    super(message)
  }
}
