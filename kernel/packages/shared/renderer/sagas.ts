import { call, fork, put } from 'redux-saga/effects'
import { signalRendererInitialized } from './actions'
import { globalDCL } from 'shared/globalDCL'

export function* rendererSaga() {
  yield fork(awaitRendererInitialSignal)
}

export function* awaitRendererInitialSignal(): any {
  yield call(waitForRenderer)
  yield put(signalRendererInitialized())
}

export async function waitForRenderer() {
  return new Promise(resolve => {
    const interval = setInterval(() => {
      if (globalDCL.rendererInterface) {
        clearInterval(interval)
        resolve()
      }
    }, 1000)
  })
}
