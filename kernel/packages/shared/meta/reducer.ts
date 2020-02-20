import { AnyAction } from 'redux'
import { META_CONFIGURATION_INITIALIZED } from './actions'
import { MetaConfigurationState } from './types'

const initialState = {
  config: {}
}

export function metaReducer(state?: MetaConfigurationState, action?: AnyAction): MetaConfigurationState {
  if (!state) {
    return initialState
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case META_CONFIGURATION_INITIALIZED:
      return {
        ...state,
        config: action.payload
      }
    default:
      return state
  }
}
