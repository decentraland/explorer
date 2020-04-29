import { ExplorerIdentity } from 'shared'
import { SocialClient, FriendshipRequest, Conversation } from 'dcl-social-client'
import { SocialAPI } from 'dcl-social-client/dist/SocialAPI'
import { Authenticator } from 'dcl-crypto'
import { takeEvery, put, select } from 'redux-saga/effects'
import { SEND_PRIVATE_MESSAGE, SendPrivateMessage, clientInitialized } from './actions'
import { getClient, findByUserId, isFriend } from './selectors'
import { createLogger } from '../logger'
import { ProfileAsPromise } from '../profiles/ProfileAsPromise'
import { unityInterface } from 'unity-interface/dcl'
import { ChatMessageType } from 'shared/types'
import { SocialData } from './types'

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

  yield put(
    clientInitialized(
      client,
      socialInfo,
      friendsSocial.map($ => $.userId),
      fromFriendRequestsSocial.map($ => $.userId),
      toFriendRequestsSocial.map($ => $.userId)
    )
  )

  yield Promise.all(
    Object.values(socialInfo)
      .map(socialData => socialData.userId)
      .map(userId => ProfileAsPromise(userId))
  )

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

  client.onFriendshipRequest(socialId => {
    // map social id to user id
    // ensure user profile is initialized and send to renderer
    // add to friendRequests
    // update renderer
  })

  client.onFriendshipRequestCancellation(socialId => {
    // map social id to user id
    // remove from friendRequests and friends
    // update renderer
  })

  client.onFriendshipRequestApproval(socialId => {
    // map social id to user id
    // add to friends if in friend request to
    // update renderer
  })

  client.onFriendshipRequestRejection(socialId => {
    // map social id to user id
    // remove from friends requests if in friend request from
    // update renderer
  })

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

function toSocialData(socialIds: string[]) {
  return socialIds
    .map(socialId => ({
      userId: parseUserId(socialId),
      socialId
    }))
    .filter(({ userId }) => !!userId) as SocialData[]
}
