import { AnyAction } from 'redux'

import { LoginStage, SessionState } from './types'
import {
  CHANGE_LOGIN_STAGE,
  ChangeSignUpStageAction,
  SIGNUP_FORM,
  SIGNUP_SET_PROFILE,
  SIGNUP_STAGE,
  SignUpFormAction,
  SignUpSetProfileAction,
  TOGGLE_WALLET_PROMPT,
  UPDATE_TOS,
  USER_AUTHENTIFIED,
  UserAuthentified
} from './actions'
import defaultLogger from '../logger'

const INITIAL_STATE: SessionState = {
  initialized: false,
  identity: undefined,
  userId: undefined,
  network: undefined,
  loginStage: LoginStage.LOADING,
  tos: false,
  showWalletPrompt: false,
  signup: {
    active: false,
    stage: '',
    tos: false,
    profile: {}
  }
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
    case SIGNUP_STAGE:
      defaultLogger.log('SIGNUP_STAGE: ', action.payload)
      return {
        ...state,
        signup: {
          ...state.signup,
          ...(action as ChangeSignUpStageAction).payload
        }
      }
    case SIGNUP_FORM:
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
    case SIGNUP_SET_PROFILE: {
      const { name, email, ...values } = (action as SignUpSetProfileAction).payload
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
  }
  return state
}
