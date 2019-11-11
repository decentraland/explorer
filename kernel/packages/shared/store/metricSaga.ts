import { takeEvery } from 'redux-saga/effects'
import { queueTrackingEvent } from '../analytics'
import { SAVE_AVATAR_SUCCESS, SaveAvatarSuccess } from '../passports/actions'
import {
  COMMS_ESTABLISHED,
  ESTABLISHING_COMMS,
  ExecutionLifecycleEvent,
  ExecutionLifecycleEventsList,
  LOADING_STARTED,
  NOT_STARTED,
  WAITING_FOR_RENDERER,
  UNITY_CLIENT_LOADED,
  LOADING_SCENES,
  EXPERIENCE_STARTED,
  TELEPORT_TRIGGERED,
  SCENE_ENTERED,
  UNEXPECTED_ERROR_LOADING_CATALOG,
  UNEXPECTED_ERROR,
  AUTH_SUCCESSFUL,
  NO_WEBGL_COULD_BE_CREATED,
  AUTH_ERROR_LOGGED_OUT,
  CONTENT_SERVER_DOWN,
  FAILED_FETCHING_UNITY,
  COMMS_ERROR_RETRYING,
  COMMS_COULD_NOT_BE_ESTABLISHED,
  MOBILE_NOT_SUPPORTED,
  NOT_INVITED,
  NEW_LOGIN
} from '../loading/types'

const trackingEvents: Record<ExecutionLifecycleEvent, string> = {
  // lifecycle events
  [NOT_STARTED]: 'session_start',
  [LOADING_STARTED]: 'loading_1_start',
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
  [UNEXPECTED_ERROR]: 'error_fatal',
  [UNEXPECTED_ERROR_LOADING_CATALOG]: 'error_catalog',
  [NO_WEBGL_COULD_BE_CREATED]: 'error_webgl',
  [AUTH_ERROR_LOGGED_OUT]: 'error_authfail',
  [CONTENT_SERVER_DOWN]: 'error_contentdown',
  [FAILED_FETCHING_UNITY]: 'error_fetchengine',
  [COMMS_ERROR_RETRYING]: 'error_comms_',
  [COMMS_COULD_NOT_BE_ESTABLISHED]: 'error_comms_failed',
  [MOBILE_NOT_SUPPORTED]: 'unsupported_mobile',
  [NOT_INVITED]: 'error_not_invited'
}

export function* metricSaga() {
  for (const event of ExecutionLifecycleEventsList) {
    yield takeEvery(event, action =>
      queueTrackingEvent('lifecycle event', toTrackingEvent(event, (action as any).payload))
    )
  }
  yield takeEvery(SAVE_AVATAR_SUCCESS, (action: SaveAvatarSuccess) =>
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

function toAvatarEditSuccess({ userId, version, profile }: SaveAvatarSuccess['payload']) {
  return { userId, version, wearables: profile.avatar.wearables }
}
