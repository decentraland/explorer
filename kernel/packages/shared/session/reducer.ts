import { AnyAction } from 'redux'

import { SessionState } from './types'
import { SIGNUP_AGREE, SIGNUP_FORM, SignUpFormAction, USER_AUTHENTIFIED, UserAuthentified } from './actions'

const INITIAL_STATE: SessionState = {
  initialized: false,
  identity: undefined,
  userId: undefined,
  network: undefined,
  signup: { name: '', email: '', tos: false }
}

export function sessionReducer(state?: SessionState, action?: AnyAction): SessionState {
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
    case SIGNUP_FORM: {
      return {
        ...state,
        signup: {
          ...state.signup,
          ...(action as SignUpFormAction).payload
        }
      }
    }
    case SIGNUP_AGREE: {
      return {
        ...state,
        signup: {
          ...state.signup,
          tos: true
        }
      }
    }
  }
  return state
}
