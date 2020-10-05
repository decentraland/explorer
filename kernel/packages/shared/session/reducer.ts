import { AnyAction } from 'redux'

import { LoginStage, SessionState } from './types'
import { ENABLE_LOGIN, UPDATE_TOS, USER_AUTHENTIFIED, UserAuthentified } from './actions'

const INITIAL_STATE: SessionState = {
  initialized: false,
  identity: undefined,
  userId: undefined,
  network: undefined,
  loginStage: LoginStage.LOADING,
  tos: false,
  showWalletPrompt: false
}

export function sessionReducer(state?: SessionState, action?: AnyAction) {
  if (!state) {
    return INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case USER_AUTHENTIFIED: {
      return { ...state, initialized: true, ...(action as UserAuthentified).payload }
    }
    case UPDATE_TOS: {
      return { ...state, tos: action.payload }
    }
    case ENABLE_LOGIN: {
      return { ...state, loginStage: LoginStage.SING_IN }
    }
  }
  return state
}
