import { applyMiddleware, compose, createStore, Store, StoreEnhancer } from 'redux'
import createSagaMiddleware from 'redux-saga'
import { createLogger } from 'redux-logger'
import { reducers } from './rootReducer'
import { createRootSaga } from './rootSaga'
import { RootState } from './rootTypes'
import { DEBUG_REDUX } from '../../config'
import { BringDownClientAndShowError, ErrorContext, ReportFatalError } from '../loading/ReportFatalError'
import defaultLogger from '../logger'

export let store: Store<RootState>

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

  store = createStore(reducers, composeEnhancers(...middlewares))
  const startSagas = () => sagaMiddleware.run(createRootSaga())
  return { store, startSagas }
}
