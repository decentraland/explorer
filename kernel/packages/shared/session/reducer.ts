import { AnyAction } from 'redux'

import { SessionState } from './types'
import {
  AUTH_ERROR,
  AuthErrorAction,
  SignSetProfileAction,
  SIGNUP_AGREE,
  SIGNUP_FORM,
  SIGNUP_SET_PROFILE,
  SignUpFormAction,
  USER_AUTHENTIFIED,
  UserAuthentified
} from './actions'

const INITIAL_STATE: SessionState = {
  initialized: false,
  identity: undefined,
  userId: undefined,
  network: undefined,
  signup: {
    tos: false,
    profile: {},
    error: null,
    errorMsg: null
  }
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
    case SIGNUP_SET_PROFILE: {
      const { name, email, ...values } = (action as SignSetProfileAction).payload
      return {
        ...state,
        signup: {
          ...state.signup,
          profile: {
            ...state.signup.profile,
            ...values
          }
        }
      }
    }
    case SIGNUP_FORM: {
      return {
        ...state,
        signup: {
          ...state.signup,
          profile: {
            ...state.signup.profile,
            ...(action as SignUpFormAction).payload
          }
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
    case AUTH_ERROR: {
      return {
        ...state,
        signup: {
          ...state.signup,
          ...(action as AuthErrorAction).payload
        }
      }
    }
  }
  return state
}
