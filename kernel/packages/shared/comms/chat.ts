import { MessageEntry, ChatMessageType, PresenceStatus } from 'shared/types'
import { uuid } from 'atomicHelpers/math'
import { StoreContainer } from '../store/rootTypes'
import { messageReceived } from '../chat/actions'
import { getProfile } from 'shared/profiles/selectors'

declare const globalThis: StoreContainer
let friendStatus: any = {}

export enum ChatEventType {
  MESSAGE_RECEIVED = 'MESSAGE_RECEIVED',
  MESSAGE_SENT = 'MESSAGE_SENT'
}

export type ChatEvent = {
  type: ChatEventType
  messageEntry: MessageEntry
}

export function notifyStatusThroughChat(status: string) {
  globalThis.globalStore.dispatch(
    messageReceived({
      messageId: uuid(),
      messageType: ChatMessageType.SYSTEM,
      timestamp: Date.now(),
      body: status
    })
  )
}

export function notifyFriendOnlineStatusThroughChat(userId: string, status: PresenceStatus) {
  const friendName = getProfile(globalThis.globalStore.getState(), userId)?.name

  if (friendName === undefined) {
    return
  }

  if (friendStatus[friendName] === undefined) {
    friendStatus[friendName] = status
    return
  }
  if (status === PresenceStatus.ONLINE && friendStatus[friendName] !== status) {
    friendStatus[friendName] = status
    globalThis.globalStore.dispatch(
      messageReceived({
        messageId: uuid(),
        messageType: ChatMessageType.SYSTEM,
        timestamp: Date.now(),
        body: `${friendName} is online.`
      })
    )
  }
}
