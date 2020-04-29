import { ExplorerIdentity } from 'shared'
import { SocialClient, FriendshipRequest, Conversation } from 'dcl-social-client'
import { SocialAPI } from 'dcl-social-client/dist/SocialAPI'
import { Authenticator } from 'dcl-crypto'
import { takeEvery, put, select, call, take } from 'redux-saga/effects'
import {
  SEND_PRIVATE_MESSAGE,
  SendPrivateMessage,
  clientInitialized,
  sendPrivateMessage,
  updateFriendship,
  UPDATE_FRIENDSHIP,
  UpdateFriendship,
  updateState,
  addUserData
} from './actions'
import { getClient, findByUserId, isFriend, getPrivateMessaging } from './selectors'
import { createLogger } from '../logger'
import { ProfileAsPromise } from '../profiles/ProfileAsPromise'
import { unityInterface } from 'unity-interface/dcl'
import { ChatMessageType, FriendshipAction } from 'shared/types'
import { SocialData, ChatState } from './types'
import { StoreContainer } from '../store/rootTypes'
import { RENDERER_INITIALIZED } from '../renderer/types'

declare const globalThis: StoreContainer & { sendPrivateMessage: (userId: string, message: string) => void }

const logger = createLogger('chat: ')

const INITIAL_CHAT_SIZE = 50

export function* initializePrivateMessaging(synapseUrl: string, identity: ExplorerIdentity) {
  const { address: ethAddress } = identity
  const timestamp = Date.now()

  const messageToSign = `${timestamp}`

  const authChain = Authenticator.signPayload(identity, messageToSign)

  const client: SocialAPI = yield SocialClient.loginToServer(synapseUrl, ethAddress, timestamp, authChain)
  const ownId = client.getUserId()

  // init friends
  const friends: string[] = yield client.getAllFriends()
  const friendsSocial: SocialData[] = yield Promise.all(
    toSocialData(friends).map(async friend => {
      const conversation = await client.createDirectConversation(friend.socialId)
      return { ...friend, conversationId: conversation.id }
    })
  )

  // init friend requests
  const friendRequests: FriendshipRequest[] = yield client.getPendingRequests()

  // filter my requests to others
  const toFriendRequests = friendRequests.filter(request => request.from === ownId).map(request => request.to)
  const toFriendRequestsSocial = toSocialData(toFriendRequests)

  // filter other requests to me
  const fromFriendRequests = friendRequests.filter(request => request.to === ownId).map(request => request.from)
  const fromFriendRequestsSocial = toSocialData(fromFriendRequests)

  const socialInfo: Record<string, SocialData> = [
    ...friendsSocial,
    ...toFriendRequestsSocial,
    ...fromFriendRequestsSocial
  ].reduce(
    (acc, current) => ({
      ...acc,
      [current.socialId]: current
    }),
    {}
  )

  const friendIds = friendsSocial.map($ => $.userId)
  const requestedFromIds = fromFriendRequestsSocial.map($ => $.userId)
  const requestedToIds = toFriendRequestsSocial.map($ => $.userId)

  yield put(clientInitialized(client, socialInfo, friendIds, requestedFromIds, requestedToIds))

  // ensure friend profiles are sent to renderer

  yield Promise.all(
    Object.values(socialInfo)
      .map(socialData => socialData.userId)
      .map(userId => ProfileAsPromise(userId))
  )

  yield take(RENDERER_INITIALIZED) // wait for renderer to initialize

  unityInterface.InitializeFriends({
    currentFriends: friendIds,
    requestedTo: requestedToIds,
    requestedFrom: requestedFromIds
  })

  // initialize conversations

  const conversations: {
    conversation: Conversation
    unreadMessages: boolean
  }[] = yield client.getAllCurrentConversations()

  yield Promise.all(
    conversations.map(async ({ conversation }) => {
      // TODO - add support for group messaging - moliva - 22/04/2020
      const cursor = await client.getCursorOnLastMessage(conversation.id, { initialSize: INITIAL_CHAT_SIZE })
      const messages = await cursor.getMessages()

      const friend = friendsSocial.find(friend => friend.conversationId === conversation.id)

      if (!friend) {
        logger.warn(`friend not found for conversation`, conversation.id)
        return
      }

      messages.forEach(message => {
        unityInterface.AddMessageToChatWindow({
          messageId: message.id,
          messageType: ChatMessageType.PRIVATE,
          timestamp: message.timestamp,
          body: message.text,
          sender: message.sender === ownId ? ethAddress : friend.userId,
          recipient: message.sender === ownId ? friend.userId : ethAddress
        })
      })
    })
  )

  yield takeEvery(UPDATE_FRIENDSHIP, handleUpdateFriendship)

  // register listener for new messages

  client.onMessage((conversation, message) => {
    const friend = friendsSocial.find(friend => friend.conversationId === conversation.id)

    if (!friend) {
      logger.warn(`friend not found for conversation`, conversation.id)
      return
    }

    unityInterface.AddMessageToChatWindow({
      messageId: message.id,
      messageType: ChatMessageType.PRIVATE,
      timestamp: message.timestamp,
      body: message.text,
      sender: message.sender === ownId ? ethAddress : friend.userId,
      recipient: message.sender === ownId ? friend.userId : ethAddress
    })
  })

  const handleIncomingFriendshipUpdateStatus = async (action: FriendshipAction, socialId: string) => {
    // map social id to user id
    const userId = parseUserId(socialId)

    if (!userId) {
      logger.warn(`cannot parse user id from social id`, socialId)
      return null
    }

    globalThis.globalStore.dispatch(addUserData(userId, socialId))

    // ensure user profile is initialized and send to renderer
    await ProfileAsPromise(userId)

    // add to friendRequests & update renderer
    globalThis.globalStore.dispatch(updateFriendship(action, userId, true))
  }

  client.onFriendshipRequest(socialId =>
    handleIncomingFriendshipUpdateStatus(FriendshipAction.REQUESTED_FROM, socialId)
  )
  client.onFriendshipRequestCancellation(socialId =>
    handleIncomingFriendshipUpdateStatus(FriendshipAction.CANCELED, socialId)
  )

  client.onFriendshipRequestApproval(socialId =>
    handleIncomingFriendshipUpdateStatus(FriendshipAction.APPROVED, socialId)
  )

  client.onFriendshipRequestRejection(socialId =>
    handleIncomingFriendshipUpdateStatus(FriendshipAction.REJECTED, socialId)
  )

  yield takeEvery(SEND_PRIVATE_MESSAGE, handleSendPrivateMessage)
}

/**
 * The social id for the time being should always be of the form `@ethAddress:server`
 *
 * @param socialId a string with the aforementioned pattern
 */
function parseUserId(socialId: string) {
  const result = socialId.match(/@(\w+):.*/)
  if (!result || result.length < 2) {
    logger.warn(`Could not match social id with ethereum address, this should not happen`)
    return null
  }
  return result[1]
}

function* handleSendPrivateMessage(action: SendPrivateMessage, debug: boolean = false) {
  const { message, userId } = action.payload

  const client: SocialAPI | null = yield select(getClient)

  if (!client) {
    logger.error(`Social client should be initialized by now`)
    return
  }

  let socialId: string
  if (!debug) {
    const userData: ReturnType<typeof findByUserId> = yield select(findByUserId, userId)
    if (!userData) {
      logger.error(`User not found ${userId}`)
      return
    }

    const _isFriend: ReturnType<typeof isFriend> = yield select(isFriend, userId)
    if (!_isFriend) {
      logger.error(`Trying to send a message to a non friend ${userId}`)
      return
    }

    socialId = userData.socialId
  } else {
    // used only for debugging purposes
    socialId = userId
  }

  const conversation: Conversation = yield client.createDirectConversation(socialId)

  const messageId: string = yield client.sendMessageTo(conversation.id, message)

  if (debug) {
    logger.info(`message sent with id `, messageId)
  }
}

function* handleUpdateFriendship({ payload, meta }: UpdateFriendship) {
  const { action, userId } = payload
  const { incoming } = meta

  const state: ReturnType<typeof getPrivateMessaging> = yield select(getPrivateMessaging)

  let newState: ChatState['privateMessaging'] | undefined

  switch (action) {
    case FriendshipAction.NONE: {
      // do nothing
      break
    }
    case FriendshipAction.APPROVED:
    case FriendshipAction.REJECTED: {
      const index = state.fromFriendRequests.indexOf(userId)

      if (index !== -1) {
        const fromFriendRequests = [...state.fromFriendRequests]
        fromFriendRequests.splice(index, 1)

        newState = { ...state, fromFriendRequests }

        if (action === FriendshipAction.APPROVED && !state.friends.includes(userId)) {
          newState.friends.push(userId)
        }
      }

      break
    }
    case FriendshipAction.CANCELED: {
      const index = state.toFriendRequests.indexOf(userId)

      if (index !== -1) {
        const toFriendRequests = [...state.toFriendRequests]
        toFriendRequests.splice(index, 1)

        newState = { ...state, toFriendRequests }
      }

      break
    }
    case FriendshipAction.REQUESTED_FROM: {
      const exists = state.fromFriendRequests.includes(userId)

      if (!exists) {
        newState = { ...state, fromFriendRequests: [...state.fromFriendRequests, userId] }
      }

      break
    }
    case FriendshipAction.REQUESTED_TO: {
      const exists = state.toFriendRequests.includes(userId)

      if (!exists) {
        newState = { ...state, toFriendRequests: [...state.toFriendRequests, userId] }
      }

      break
    }
    case FriendshipAction.DELETED: {
      const index = state.friends.indexOf(userId)

      if (index !== -1) {
        const friends = [...state.friends]
        friends.splice(index, 1)

        newState = { ...state, friends }
      }

      break
    }
  }

  if (newState) {
    yield put(updateState(newState))

    if (incoming) {
      unityInterface.UpdateFriendshipStatus(payload)
    } else {
      yield call(handleOutgoingUpdateFriendshipStatus, payload)
    }
  }
}

function* handleOutgoingUpdateFriendshipStatus(update: UpdateFriendship['payload']) {
  const client: SocialAPI = yield select(getClient)
  const socialData: SocialData = yield select(findByUserId, update.userId)

  if (!socialData) {
    logger.error(`could not find social data for`, update.userId)
    return
  }

  switch (update.action) {
    case FriendshipAction.NONE: {
      // do nothing in this case
      break
    }
    case FriendshipAction.APPROVED: {
      yield client.addAsFriend(socialData.socialId)

      break
    }
    case FriendshipAction.REJECTED: {
      break
    }
    case FriendshipAction.CANCELED: {
      break
    }
    case FriendshipAction.REQUESTED_FROM: {
      break
    }
    case FriendshipAction.REQUESTED_TO: {
      break
    }
    case FriendshipAction.DELETED: {
      break
    }
  }
}

function toSocialData(socialIds: string[]) {
  return socialIds
    .map(socialId => ({
      userId: parseUserId(socialId),
      socialId
    }))
    .filter(({ userId }) => !!userId) as SocialData[]
}

globalThis.sendPrivateMessage = (userId: string, message: string) =>
  handleSendPrivateMessage(sendPrivateMessage(userId, message), true)
