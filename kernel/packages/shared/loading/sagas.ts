import { AnyAction } from 'redux'
import { fork, put, race, select, take, takeEvery } from 'redux-saga/effects'

import { PARCEL_LOADING_STARTED, RENDERER_INITIALIZED } from 'shared/renderer/types'
import { ChangeLoginStateAction, CHANGE_LOGIN_STAGE } from 'shared/session/actions'
import { trackEvent } from '../analytics'
import { lastPlayerPosition } from '../world/positionThings'

import { PENDING_SCENES, SceneLoad, SCENE_FAIL, SCENE_LOAD, SCENE_START } from './actions'
import { metricsUnityClientLoaded, metricsAuthSuccessful, experienceStarted } from './types'
import { getCurrentUserId } from 'shared/session/selectors'
import { LoginState } from '@dcl/kernel-interface'
import { call } from 'redux-saga-test-plan/matchers'
import { RootState } from 'shared/store/rootTypes'
import { onLoginCompleted } from 'shared/session/sagas'

export function* loadingSaga() {
  yield fork(translateActions)
  yield fork(initialSceneLoading)

  yield takeEvery(SCENE_LOAD, trackLoadTime)
}

function* translateActions() {
  yield takeEvery(RENDERER_INITIALIZED, triggerUnityClientLoaded)
  yield takeEvery(CHANGE_LOGIN_STAGE, triggerAuthSuccessful)
}

function* triggerAuthSuccessful(action: ChangeLoginStateAction) {
  if (action.payload.stage === LoginState.COMPLETED) {
    yield put(metricsAuthSuccessful())
  }
}

function* triggerUnityClientLoaded() {
  yield put(metricsUnityClientLoaded())
}

export function* trackLoadTime(action: SceneLoad): any {
  const start = new Date().getTime()
  const sceneId = action.payload
  const result = yield race({
    start: take((action: AnyAction) => action.type === SCENE_START && action.payload === sceneId),
    fail: take((action: AnyAction) => action.type === SCENE_FAIL && action.payload === sceneId)
  })
  const userId = yield select(getCurrentUserId)
  const position = lastPlayerPosition
  trackEvent('SceneLoadTimes', {
    position: { ...position },
    elapsed: new Date().getTime() - start,
    success: !!result.start,
    sceneId,
    userId: userId
  })
}

function* waitForSceneLoads() {
  function shouldWaitForScenes(state: RootState) {
    if (!state.renderer.parcelLoadingStarted) {
      return true
    }

    // in the initial load, we should wait until we have *some* scene to load
    if (state.loading.initialLoad) {
      if (state.loading.pendingScenes !== 0 || state.loading.totalScenes === 0) {
        return true
      }
    }

    // otherwise only wait until pendingScenes == 0
    return state.loading.pendingScenes !== 0
  }

  while (yield select(shouldWaitForScenes)) {
    yield race({
      pendingScenes: take(PENDING_SCENES),
      sceneLoading: take(PARCEL_LOADING_STARTED)
    })
  }
}

function* initialSceneLoading() {
  yield onLoginCompleted()
  yield call(waitForSceneLoads)
  yield put(experienceStarted())
}
