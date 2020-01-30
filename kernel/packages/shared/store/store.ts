import { applyMiddleware, compose, createStore, Store } from 'redux'
import createSagaMiddleware from 'redux-saga'
import { reducers } from './rootReducer'
import { createRootSaga } from './rootSaga'
import { RootState } from './rootTypes'
import { DEBUG_REDUX } from '../../config'

export let store: Store<RootState>

export const buildStore = (config: {
  ephemeralKeyTTL: number
  clientId: string
  domain: string
  redirectUri: string
  audience: string
}) => {
  const sagaMiddleware = createSagaMiddleware()
  const composeEnhancers = (DEBUG_REDUX && (window as any).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__) || compose
  store = createStore(reducers, composeEnhancers(applyMiddleware(sagaMiddleware)))
  const startSagas = () => sagaMiddleware.run(createRootSaga())
  return { store, startSagas }
}
