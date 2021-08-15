import { call, put, select, take } from 'redux-saga/effects'
import { waitingForRenderer } from 'shared/loading/types'
import { initializeEngine } from 'unity-interface/dcl'
import type { UnityGame } from '@dcl/unity-renderer/src/index'
import { signalRendererInitialized, InitializeRenderer, INITIALIZE_RENDERER } from './actions'
import { isInitialized } from './selectors'
import { RENDERER_INITIALIZED } from './types'

export function* rendererSaga() {
  const action: InitializeRenderer = yield take(INITIALIZE_RENDERER)
  yield call(initializeRenderer, action)
}

export function* waitForRendererInstance() {
  while (!(yield select(isInitialized))) {
    yield take(RENDERER_INITIALIZED)
  }
}

let handlerFunction: (type: string, jsonEncodedMessage: string) => void = () => void 0

export function setMessageFromEngineHandler(fn: typeof handlerFunction) {
  handlerFunction = fn
}

function* initializeRenderer(action: InitializeRenderer) {
  const { delegate, container } = action.payload

  // will start loading
  yield put(waitingForRenderer())

  // start loading the renderer
  const renderer: UnityGame = yield delegate(container)

  // wire the kernel to the renderer
  yield call(initializeEngine, renderer)

  // send an "engineStarted" notification
  yield put(signalRendererInitialized())

  return renderer
}
