import { SessionState } from './types'
import { AnyAction } from 'redux'
import { LOGIN_COMPLETED, LoginCompleted } from './actions'

const INITIAL_STATE: SessionState = {
  initialized: false,
  identity: undefined,
  userId: undefined,
  network: undefined
}

export function sessionReducer(state?: SessionState, action?: AnyAction) {
  if (!state) {
    return INITIAL_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case LOGIN_COMPLETED: {
      return { ...state, initialized: true, ...(action as LoginCompleted).payload }
    }
  }
  return state
}
