import { AnyAction } from 'redux'
import { ChatState } from './types'
import {
  SOCIAL_CLIENT_INITIALIZED,
  ClientInitialized,
  UPDATE_PRIVATE_MESSAGING,
  UpdateState,
  ADD_USER_DATA,
  AddUserData
} from './actions'

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
    case UPDATE_PRIVATE_MESSAGING: {
      return reducePrivateMessaging(state, action as UpdateState)
    }
    case ADD_USER_DATA: {
      return reduceAddUserData(state, action as AddUserData)
    }
  }
  return state
}

function reduceSocialClientInitialized(state: ChatState, action: ClientInitialized) {
  return { ...state, privateMessaging: action.payload }
}

function reducePrivateMessaging(state: ChatState, action: UpdateState) {
  return { ...state, privateMessaging: action.payload }
}

function reduceAddUserData(state: ChatState, action: AddUserData) {
  return {
    ...state,
    privateMessaging: {
      ...state.privateMessaging,
      socialInfo: {
        ...state.privateMessaging.socialInfo,
        [action.payload.socialId]: action.payload
      }
    }
  }
}
