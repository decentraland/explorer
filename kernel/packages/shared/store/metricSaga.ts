import { action } from 'typesafe-actions'
import { takeEvery } from 'redux-saga/effects'
import { queueTrackingEvent } from '../analytics'
import { SAVE_PROFILE_SUCCESS, SaveProfileSuccess } from '../profiles/actions'
import {
  AUTH_ERROR_LOGGED_OUT,
  AUTH_SUCCESSFUL,
  AWAITING_USER_SIGNATURE,
  CATALYST_COULD_NOT_LOAD,
  COMMS_COULD_NOT_BE_ESTABLISHED,
  COMMS_ERROR_RETRYING,
  COMMS_ESTABLISHED,
  CONTENT_SERVER_DOWN,
  ESTABLISHING_COMMS,
  ExecutionLifecycleEvent,
  ExecutionLifecycleEventsList,
  EXPERIENCE_STARTED,
  FAILED_FETCHING_UNITY,
  LOADING_SCENES,
  LOADING_STARTED,
  MOBILE_NOT_SUPPORTED,
  NETWORK_MISMATCH,
  NEW_LOGIN,
  NO_WEBGL_COULD_BE_CREATED,
  NOT_INVITED,
  NOT_STARTED,
  SCENE_ENTERED,
  TELEPORT_TRIGGERED,
  UNEXPECTED_ERROR,
  UNEXPECTED_ERROR_LOADING_CATALOG,
  UNITY_CLIENT_LOADED,
  WAITING_FOR_RENDERER
} from '../loading/types'

const trackingEvents: Record<ExecutionLifecycleEvent, string> = {
  // lifecycle events
  [NOT_STARTED]: 'session_start',
  [LOADING_STARTED]: 'loading_1_start',
  [AWAITING_USER_SIGNATURE]: 'loading_1_1_awaiting_user_signature',
  [AUTH_SUCCESSFUL]: 'loading_2_authOK',
  [ESTABLISHING_COMMS]: 'loading_3_init_comms',
  [COMMS_ESTABLISHED]: 'loading_4_comms_established',
  [WAITING_FOR_RENDERER]: 'loading_5_wait_renderer',
  [UNITY_CLIENT_LOADED]: 'loading_6_unity_ok',
  [LOADING_SCENES]: 'loading_7_load_scenes',
  [EXPERIENCE_STARTED]: 'loading_8_finished',
  [TELEPORT_TRIGGERED]: 'teleport_triggered',
  [SCENE_ENTERED]: 'scene_entered',
  [NEW_LOGIN]: 'new_login',
  // errors
  [NETWORK_MISMATCH]: 'network_mismatch',
  [UNEXPECTED_ERROR]: 'error_fatal',
  [UNEXPECTED_ERROR_LOADING_CATALOG]: 'error_catalog',
  [NO_WEBGL_COULD_BE_CREATED]: 'error_webgl',
  [AUTH_ERROR_LOGGED_OUT]: 'error_authfail',
  [CONTENT_SERVER_DOWN]: 'error_contentdown',
  [FAILED_FETCHING_UNITY]: 'error_fetchengine',
  [COMMS_ERROR_RETRYING]: 'error_comms_',
  [COMMS_COULD_NOT_BE_ESTABLISHED]: 'error_comms_failed',
  [CATALYST_COULD_NOT_LOAD]: 'error_catalyst_loading',
  [MOBILE_NOT_SUPPORTED]: 'unsupported_mobile',
  [NOT_INVITED]: 'error_not_invited'
}
const GENERIC_ERROR = '[GENERIC_ERROR] track a generic error'
export const genericError = (error: any) => action(GENERIC_ERROR, { error })
export type GenericErrorAction = ReturnType<typeof genericError>

export function* metricSaga() {
  yield takeEvery(GENERIC_ERROR, trackGenericError)
  for (const event of ExecutionLifecycleEventsList) {
    yield takeEvery(event, (action) => {
      const _action: any = action
      queueTrackingEvent('lifecycle event', toTrackingEvent(event, _action.payload))
    })
  }
  yield takeEvery(SAVE_PROFILE_SUCCESS, (action: SaveProfileSuccess) =>
    queueTrackingEvent('avatar_edit_success', toAvatarEditSuccess(action.payload))
  )
}

function toTrackingEvent(event: ExecutionLifecycleEvent, payload: any) {
  let result = trackingEvents[event]
  if (event === COMMS_ERROR_RETRYING) {
    result += payload
  }
  return { stage: result }
}

function toAvatarEditSuccess({ userId, version, profile }: SaveProfileSuccess['payload']) {
  return { userId, version, wearables: profile.avatar.wearables }
}

function* trackGenericError(action: GenericErrorAction) {
  queueTrackingEvent('generic_error', { ...action.payload })
}
