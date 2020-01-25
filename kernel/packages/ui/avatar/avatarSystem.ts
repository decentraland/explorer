import { Component, engine, Entity, log, Observable, ProfileForRenderer, Transform } from 'decentraland-ecs/src'
import { AvatarShape } from 'decentraland-ecs/src/decentraland/AvatarShape'
import {
  AvatarMessage,
  AvatarMessageType,
  Pose,
  ReceiveUserDataMessage,
  ReceiveUserPoseMessage,
  ReceiveUserVisibleMessage,
  UserRemovedMessage,
  UserMessage
} from 'shared/comms/interface/types'

export const avatarMessageObservable = new Observable<AvatarMessage>()

@Component('AvatarState')
class AvatarState {
  blocked = false
  muted = false
  visible = true
}

@Component('AvatarSceneStatus')
class AvatarSceneStatus {
  pendingMessages: AvatarMessage[] = []
  avatarMap = new Map<string, Entity>()
}

const globalAvatarContainer = new Entity()
engine.addEntity(globalAvatarContainer)
const status = new AvatarSceneStatus()
globalAvatarContainer.addComponent(status)

avatarMessageObservable.add(evt => {
  status.pendingMessages.push(evt)
})

export class AvatarSystem {
  update() {
    const avatarSceneStatus = globalAvatarContainer.getComponent(AvatarSceneStatus)
    const pendings = avatarSceneStatus.pendingMessages

    for (let message of pendings) {
      switch (message.type) {
        case AvatarMessageType.USER_DATA:
          this.handleUserData(avatarSceneStatus, message)
          break
        case AvatarMessageType.USER_POSE:
          this.handleUserPose(avatarSceneStatus, message)
          break
        case AvatarMessageType.USER_VISIBLE:
          this.handleUserVisible(avatarSceneStatus, message)
          break
        case AvatarMessageType.USER_REMOVED:
          this.handleUserRemoved(avatarSceneStatus, message)
          break
        case AvatarMessageType.USER_BLOCKED:
        case AvatarMessageType.USER_UNBLOCKED:
          this.handleBlockedMessages(avatarSceneStatus, message)
          break
        case AvatarMessageType.SHOW_WINDOW:
          // handleShowWindow(avatarSceneStatus, message)
          break
      }
    }

    // Clear array after we're done
    pendings.length = 0
  }

  handleUserData(avatarStatus: AvatarSceneStatus, message: ReceiveUserDataMessage) {
    if (!message.data) {
      log(`Error: received a message with userdata but no data was provided for avatar ${message.uuid}`)
      return
    }
    const avatarEntity = this.ensureAvatar(avatarStatus, message.uuid)
    if (message.data.profile) {
      this.setAvatarInfo(avatarEntity, message.data.profile)
    }
    if (message.data.pose) {
      this.setUserPose(avatarEntity, message.data.pose)
    }
    this.checkVisibility(avatarEntity)
  }

  handleUserPose(avatarStatus: AvatarSceneStatus, message: ReceiveUserPoseMessage) {
    const avatarEntity = this.ensureAvatar(avatarStatus, message.uuid)
    this.setUserPose(avatarEntity, message.pose)
  }

  handleUserVisible(avatarStatus: AvatarSceneStatus, message: ReceiveUserVisibleMessage) {
    const avatarEntity = this.ensureAvatar(avatarStatus, message.uuid)
    this.setVisible(avatarEntity, message.visible)
  }

  handleUserRemoved(avatarStatus: AvatarSceneStatus, message: UserRemovedMessage) {
    const avatar = this.ensureAvatar(avatarStatus, message.uuid)
    avatarStatus.avatarMap.delete(message.uuid)
    engine.removeEntity(avatar)
  }

  handleBlockedMessages(avatarStatus: AvatarSceneStatus, message: UserMessage) {
    const avatar = this.ensureAvatar(avatarStatus, message.uuid)
    const state = avatar.getComponent(AvatarState)
    if (message.type === AvatarMessageType.USER_BLOCKED) {
      state.blocked = true
      this.checkVisibility(avatar)
    }
    if (message.type === AvatarMessageType.USER_UNBLOCKED) {
      state.blocked = false
      this.checkVisibility(avatar)
    }
  }

  ensureAvatar(avatarState: AvatarSceneStatus, uuid: string) {
    let avatar = avatarState.avatarMap.get(uuid)
    if (avatar) {
      return avatar
    }
    avatar = new Entity(uuid)
    avatar.getComponentOrCreate(Transform)
    avatar.getComponentOrCreate(AvatarState)
    avatar.getComponentOrCreate(AvatarShape)
    avatarState.avatarMap.set(uuid, avatar)
    return avatar
  }

  setAvatarInfo(avatarEntity: Entity, profile: ProfileForRenderer) {
    const avatarShape = avatarEntity.getComponent(AvatarShape)
    avatarShape.id = profile.userId
    avatarShape.name = profile.name
    avatarShape.bodyShape = profile.avatar.bodyShape
    avatarShape.wearables = profile.avatar.wearables
    avatarShape.skinColor = profile.avatar.skinColor
    avatarShape.hairColor = profile.avatar.hairColor
    avatarShape.eyeColor = profile.avatar.eyeColor
  }

  setUserPose(avatarEntity: Entity, pose: Pose) {
    const transform = avatarEntity.getComponent(Transform)
    const [x, y, z, Qx, Qy, Qz, Qw] = pose

    transform.position.set(x, y, z)
    transform.rotation.set(Qx, Qy, Qz, Qw)
  }

  setBlocked(avatarEntity: Entity, blocked: boolean, muted: boolean): void {
    const state = avatarEntity.getComponent(AvatarState)
    state.blocked = blocked
    state.muted = muted

    this.checkVisibility(avatarEntity)
  }

  setVisible(avatarEntity: Entity, visible: boolean): void {
    const state = avatarEntity.getComponent(AvatarState)
    state.visible = visible

    this.checkVisibility(avatarEntity)
  }

  checkVisibility(avatarEntity: Entity) {
    const state = avatarEntity.getComponent(AvatarState)
    if (state.visible && !state.blocked) {
      if (!avatarEntity.isAddedToEngine()) {
        engine.addEntity(avatarEntity)
      }
    } else {
      if (avatarEntity.isAddedToEngine()) {
        engine.removeEntity(avatarEntity)
      }
    }
  }
}

engine.addSystem(new AvatarSystem())
