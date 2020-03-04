import { expectSaga } from 'redux-saga-test-plan'
import { bootstrapAt, processUserTeleport } from './actions'
import { fetchScenesFromServer } from './download/fetchV3'
import { rootReducer } from './reducer'
import { rootSaga } from './saga'
import { genesis } from './test/serverResult'

describe('rootSaga', () => {
  it('integration test works', () => {
    return expectSaga(rootSaga)
      .provide({
        call(effect, next) {
          if (effect.fn === fetchScenesFromServer) {
            return genesis
          }
          return next()
        }
      })
      .dispatch(bootstrapAt('5,5'))
      .withReducer(rootReducer)
      .silentRun(10)
  })

  it('integration test: teleport', () => {
    return expectSaga(rootSaga)
      .provide({
        call(effect, next) {
          if (effect.fn === fetchScenesFromServer) {
            return genesis
          }
          return next()
        }
      })
      .dispatch(bootstrapAt('5,5'))
      .delay(50)
      .dispatch(processUserTeleport('15,15'))
      .delay(60)
      .dispatch(processUserTeleport('35,15'))
      .withReducer(rootReducer)
      .silentRun(100)
  })
})
