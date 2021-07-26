import { applyMiddleware, compose, createStore, StoreEnhancer } from 'redux'
import createSagaMiddleware from 'redux-saga'
import { createLogger } from 'redux-logger'
import { reducers } from './rootReducer'
import { createRootSaga } from './rootSaga'
import { DEBUG_REDUX } from '../../config'
import { BringDownClientAndShowError, ErrorContext, ReportFatalError } from '../loading/ReportFatalError'
import defaultLogger from '../logger'
import { setStore } from './isolatedStore'

export const buildStore = () => {
  const sagaMiddleware = createSagaMiddleware({
    onError: (error: Error, { sagaStack }: { sagaStack: string }) => {
      defaultLogger.log('SAGA-ERROR: ', error)
      ReportFatalError(error, ErrorContext.KERNEL_SAGA, { sagaStack })
      BringDownClientAndShowError(error.message as any)
    }
  })
  const composeEnhancers = (DEBUG_REDUX && (window as any).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__) || compose

  let middlewares: StoreEnhancer<any>[] = [applyMiddleware(sagaMiddleware)]

  if (DEBUG_REDUX) {
    middlewares.unshift(
      applyMiddleware(
        createLogger({
          collapsed: true,
          stateTransformer: () => null
        })
      )
    )
  }

  const store = createStore(reducers, composeEnhancers(...middlewares))
  const startSagas = () => sagaMiddleware.run(createRootSaga())
  setStore(store)
  return { store, startSagas }
}
