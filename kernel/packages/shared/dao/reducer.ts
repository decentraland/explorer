import { AnyAction } from 'redux'
import { SET_KATALYST_REALM } from './actions'
import { DaoState } from './types'

export function daoReducer(state?: DaoState, action?: AnyAction): DaoState {
  if (!state) {
    return {
      initialized: false,
      profileServer: '',
      fetchContentServer: '',
      updateContentServer: '',
      commsServer: '',
      layer: ''
    }
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case SET_KATALYST_REALM:
      return {
        ...state,
        initialized: true,
        profileServer: action.payload.domain + '/lambdas/profile',
        fetchContentServer: action.payload.domain + '/lambdas/contentv2',
        updateContentServer: action.payload.domain + '/content',
        commsServer: action.payload.domain + '/comms',
        layer: action.payload.layer
      }
    default:
      return state
  }
}
