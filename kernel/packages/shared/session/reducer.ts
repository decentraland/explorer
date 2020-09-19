import { AnyAction } from 'redux'

import { SessionState } from './types'
import {
  AUTH_ERROR,
  AUTH_CLEAR_ERROR,
  AuthErrorAction,
  SignSetProfileAction,
  SIGNUP_ACTIVE,
  SIGNUP_AGREE,
  SIGNUP_FORM,
  SIGNUP_SET_PROFILE,
  SignUpActiveAction,
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
    active: false,
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
    case SIGNUP_ACTIVE: {
      return {
        ...state,
        signup: {
          ...state.signup,
          active: (action as SignUpActiveAction).payload
        }
      }
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
      console.log('ERROR: ', action.payload)
      return {
        ...state,
        signup: {
          ...state.signup,
          ...(action as AuthErrorAction).payload
        }
      }
    }
    case AUTH_CLEAR_ERROR: {
      return {
        ...state,
        signup: {
          ...state.signup,
          error: null,
          errorMsg: null
        }
      }
    }
  }
  return state
}
