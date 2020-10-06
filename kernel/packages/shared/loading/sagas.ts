import { AnyAction } from 'redux'
import { call, fork, put, race, select, take, takeEvery, takeLatest } from 'redux-saga/effects'

import { RENDERER_INITIALIZED } from 'shared/renderer/types'
import { LOGIN_COMPLETED, USER_AUTHENTIFIED } from 'shared/session/actions'
import { web3initialized } from 'shared/dao/actions'
import { queueTrackingEvent } from '../analytics'
import { getCurrentUser } from '../comms/peers'
import { lastPlayerPosition } from '../world/positionThings'

import { SCENE_FAIL, SCENE_LOAD, SCENE_START, SceneLoad } from './actions'
import { authSuccessful, EXPERIENCE_STARTED, setLoadingScreen, TELEPORT_TRIGGERED, unityClientLoaded } from './types'

const SECONDS = 1000

export const DELAY_BETWEEN_MESSAGES = 10 * SECONDS

export function* loadingSaga() {
  yield fork(translateActions)

  yield fork(initialSceneLoading)
  yield takeLatest(TELEPORT_TRIGGERED, teleportSceneLoading)

  yield takeEvery(SCENE_LOAD, trackLoadTime)
}

function* translateActions() {
  yield takeEvery(RENDERER_INITIALIZED, triggerUnityClientLoaded)
  yield takeEvery(USER_AUTHENTIFIED, triggerWeb3Initialized)
  yield takeEvery(LOGIN_COMPLETED, triggerAuthSuccessful)
}

function* triggerAuthSuccessful() {
  yield put(authSuccessful())
}

function* triggerWeb3Initialized() {
  yield put(web3initialized())
}

function* triggerUnityClientLoaded() {
  yield put(unityClientLoaded())
}

export function* trackLoadTime(action: SceneLoad): any {
  const start = new Date().getTime()
  const sceneId = action.payload
  const result = yield race({
    start: take((action: AnyAction) => action.type === SCENE_START && action.payload === sceneId),
    fail: take((action: AnyAction) => action.type === SCENE_FAIL && action.payload === sceneId)
  })
  const user = yield select(getCurrentUser)
  const position = lastPlayerPosition
  queueTrackingEvent('SceneLoadTimes', {
    position: { ...position },
    elapsed: new Date().getTime() - start,
    success: !!result.start,
    sceneId,
    userId: user.userId
  })
}

export function* waitForSceneLoads() {
  while (true) {
    yield race({
      started: take(SCENE_START),
      failed: take(SCENE_FAIL)
    })
    if (yield select((state) => state.loading.pendingScenes === 0)) {
      break
    }
  }
}

export function* initialSceneLoading() {
  yield call(function* () {
    yield take(EXPERIENCE_STARTED)
    yield put(setLoadingScreen(false))
  })
}

export function* teleportSceneLoading() {
  yield call(waitForSceneLoads)
}
