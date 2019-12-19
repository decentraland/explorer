import { ProfileForRenderer } from 'decentraland-ecs/src'

export enum AvatarMessageType {
  // Networking related messages
  USER_DATA = 'USER_DATA',
  USER_POSE = 'USER_POSE',
  USER_VISIBLE = 'USER_VISIBLE',
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
  profile?: ProfileForRenderer
}

// The order is [X,Y,Z,Qx,Qy,Qz,Qw]
export type Pose = [number, number, number, number, number, number, number]

export type PoseInformation = {
  v: Pose
}

// message AuthData {
//     string signature = 1;
//     string identity = 2;
//     string timestamp = 3;
//     string access_token = 4;
// }

// enum Category {
//      UNKNOWN = 0;
//      POSITION = 1;
//      PROFILE = 2;
//      CHAT = 3;
//      SCENE_MESSAGE = 4;
// }

// message DataHeader {
//     Category category = 1;
// }

// message PositionData {
//     Category category = 1;
//     double time = 2;
//     float position_x = 3;
//     float position_y = 4;
//     float position_z = 5;
//     float rotation_x = 6;
//     float rotation_y = 7;
//     float rotation_z = 8;
//     float rotation_w = 9;
// }
export type Package<T> = {
  time: number
  data: T
}

// message ProfileData {
//     Category category = 1;
//     double time = 2;
//     string profile_version = 3;
// }

// message ChatData {
//     Category category = 1;
//     double time = 2;
//     string message_id = 3;
//     string text = 4;
// }
