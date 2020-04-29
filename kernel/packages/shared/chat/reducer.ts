import { AnyAction } from 'redux'
import { ChatState } from './types'
import { SOCIAL_CLIENT_INITIALIZED, ClientInitialized } from './actions'

const CHAT_INITIAL_STATE: ChatState = {
  privateMessaging: {
    client: null,
    socialInfo: {},
    friends: [],
    toFriendRequests: [],
    fromFriendRequests: []
  }
}

export function chatReducer(state?: ChatState, action?: AnyAction) {
  if (!state) {
    return CHAT_INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SOCIAL_CLIENT_INITIALIZED: {
      return reduceSocialClientInitialized(state, action as ClientInitialized)
    }
  }
  return state
}

function reduceSocialClientInitialized(state: ChatState, action: ClientInitialized) {
  return { ...state, privateMessaging: action.payload }
}
