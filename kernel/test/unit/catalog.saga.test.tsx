import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import {
  handleWearablesFailure,
  handleWearablesRequest,
  handleWearablesSuccess,
  informRequestFailure,
  sendWearablesCatalog,
  WRONG_FILTERS_ERROR
} from 'shared/catalogs/sagas'
import { wearablesFailure, wearablesRequest, wearablesSuccess } from 'shared/catalogs/actions'
import { baseCatalogsLoaded, getPlatformCatalog } from 'shared/catalogs/selectors'
import { waitForRendererInstance } from 'shared/renderer/sagas'

const serverUrl = 'https://server.com'
const wearableId1 = 'WearableId1'
const wearable1 = {
  id: wearableId1,
  baseUrl: serverUrl + 'contents/',
  baseUrlBundles: 'https://content-assets-as-bundle.decentraland.zone/'
} as any

const userId = 'userId'
const context = 'someContext'

describe('Wearables Saga', () => {
  it('When wearables fetch is successful, then it is sent to the renderer with the same context', () => {
    const wearables = [wearable1]
    return expectSaga(handleWearablesSuccess, wearablesSuccess(wearables, context))
      .call(sendWearablesCatalog, wearables, context)
      .provide([
        [call(waitForRendererInstance), true],
        [call(sendWearablesCatalog, wearables, context), null]
      ])
      .run()
  })

  it('When more than one filter is set, then the request fails', () => {
    return expectSaga(
      handleWearablesRequest,
      wearablesRequest({ wearableIds: ['some-id'], ownedByUser: userId }, context)
    )
      .put(wearablesFailure(context, WRONG_FILTERS_ERROR))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getPlatformCatalog), {}]
      ])
      .run()
  })

  it('When collection id is not base-avatars, then the request fails', () => {
    return expectSaga(handleWearablesRequest, wearablesRequest({ collectionIds: ['some-other-collection'] }, context))
      .put(wearablesFailure(context, WRONG_FILTERS_ERROR))
      .provide([[select(baseCatalogsLoaded), true]])
      .run()
  })

  it('When request fails, then the failure is informed', () => {
    const errorMessage = 'Something failed'
    return expectSaga(handleWearablesFailure, wearablesFailure(context, errorMessage))
      .call(informRequestFailure, errorMessage, context)
      .provide([
        [call(waitForRendererInstance), true],
        [call(informRequestFailure, errorMessage, context), null]
      ])
      .run()
  })
})
