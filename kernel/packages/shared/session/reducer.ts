import { AnyAction } from 'redux'

import { LoginStage, SessionState } from './types'
import { CHANGE_LOGIN_STAGE, TOGGLE_WALLET_PROMPT, UPDATE_TOS, USER_AUTHENTIFIED, UserAuthentified } from './actions'

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
    case CHANGE_LOGIN_STAGE: {
      return { ...state, loginStage: action.payload.stage }
    }
    case TOGGLE_WALLET_PROMPT:
      return { ...state, showWalletPrompt: action.payload.show }
  }
  return state
}
