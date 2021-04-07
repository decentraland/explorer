import { call, put, select, take } from 'redux-saga/effects'
import { waitingForRenderer } from 'shared/loading/types'
import { createLogger } from 'shared/logger'
import { browserInterface } from 'unity-interface/BrowserInterface'
import { initializeEngine, RendererInterfaces, setLoadingScreenVisible } from 'unity-interface/dcl'
import { UnityGame } from 'unity-interface/loader'
import { engineStarted, InitializeRenderer, INITIALIZE_RENDERER } from './actions'
import { isInitialized } from './selectors'
import { RENDERER_INITIALIZED } from './types'

const DEBUG = false
const logger = createLogger('renderer: ')

export function* rendererSaga() {
  const action: InitializeRenderer = yield take(INITIALIZE_RENDERER)
  yield call(initializeRenderer, action)
}

export function* ensureRenderer() {
  while (!(yield select(isInitialized))) {
    yield take(RENDERER_INITIALIZED)
  }
}

function* initializeRenderer(action: InitializeRenderer) {
  const { delegate, container } = action.payload

  setLoadingScreenVisible(true)

  let instancedJS: RendererInterfaces | null = null

  // will start loading
  yield put(waitingForRenderer())

  // async start loading
  let rendererFuture = delegate(container, handleMessageFromEngine)

  // We have to wait ENGINE_STARTED at the same time we fire off the async instantiate
  // otherwise we get a race condition because ENGINE_STARTED gets fired off as soon
  // instantiate is resolved.

  // wait for the UnityGame instance, it means the renderer is connected and working
  const renderer: UnityGame = yield rendererFuture

  instancedJS = yield initializeEngine(renderer)

  // send an "engineStarted" notification
  yield put(engineStarted())

  return renderer
}

function handleMessageFromEngine(type: string, jsonEncodedMessage: string) {
  DEBUG && logger.info(`handleMessageFromEngine`, type)
  if (browserInterface) {
    let parsedJson = null
    try {
      parsedJson = JSON.parse(jsonEncodedMessage)
    } catch (e) {
      // we log the whole message to gain visibility
      logger.error(e.message + ' messageFromEngine: ' + type + ' ' + jsonEncodedMessage)
      throw e
    }
    // this is outside of the try-catch to enable V8 path optimizations
    // keep the following line outside the `try`
    browserInterface.handleUnityMessage(type, parsedJson)
  } else {
    logger.error('Message received without initializing engine', type, jsonEncodedMessage)
  }
}
