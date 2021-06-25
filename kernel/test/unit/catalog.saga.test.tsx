import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import { BASE_AVATARS_COLLECTION_ID, handleWearablesFailure, handleWearablesRequest, handleWearablesSuccess, informRequestFailure, sendWearablesCatalog, WRONG_FILTERS_ERROR } from 'shared/catalogs/sagas'
import { wearablesFailure, wearablesRequest, wearablesSuccess } from 'shared/catalogs/actions'
import { baseCatalogsLoaded, getExclusiveCatalog, getPlatformCatalog } from 'shared/catalogs/selectors'
import { ensureRenderer } from 'shared/renderer/sagas'
import { getFetchContentServer } from 'shared/dao/selectors'

const serverUrl = 'https://server.com'
const wearableId1 = 'WearableId1'
const wearable1 = { id: wearableId1, baseUrl: serverUrl + 'contents/', baseUrlBundles: "https://content-assets-as-bundle.decentraland.zone/" } as any

const userId = 'userId'
const context = 'someContext'


describe('Wearables Saga', () => {

  it('When a wearable is requested by id, then it is returned successfully', () => {
    return expectSaga(handleWearablesRequest, wearablesRequest({ wearableIds: [wearableId1] }, context))
      .put(wearablesSuccess([wearable1], context))
      .provide([
        [select(getFetchContentServer), serverUrl],
        [select(baseCatalogsLoaded), true],
        [select(getPlatformCatalog), {}],
        [select(getExclusiveCatalog), { [wearableId1]: wearable1 }],
      ])
      .run()
  })

  it('When base avatars are requested, then they are returned successfully', () => {
    const baseWearables = [wearable1]
    return expectSaga(handleWearablesRequest, wearablesRequest({ collectionIds: [BASE_AVATARS_COLLECTION_ID] }, context))
      .put(wearablesSuccess(baseWearables, context))
      .provide([
        [select(getFetchContentServer), serverUrl],
        [select(baseCatalogsLoaded), true],
        [select(getPlatformCatalog), { [wearableId1]: wearable1 }],
        [select(getExclusiveCatalog), {}],
      ])
      .run()
  })


  it('When wearables fetch is successful, then it is sent to the renderer with the same context', () => {
    const wearables = [wearable1]
    return expectSaga(handleWearablesSuccess, wearablesSuccess(wearables, context))
      .call(sendWearablesCatalog, wearables, context)
      .provide([
        [call(ensureRenderer), true],
        [call(sendWearablesCatalog, wearables, context), null],
      ])
      .run()
  })

  it('When more than one filter is set, then the request fails', () => {
    return expectSaga(handleWearablesRequest, wearablesRequest({ wearableIds: ['some-id'], ownedByUser: userId }, context))
      .put(wearablesFailure(context, WRONG_FILTERS_ERROR))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getPlatformCatalog), {}],
        [select(getExclusiveCatalog), {}],
      ])
      .run()
  })

  it('When collection id is not base-avatars, then the request fails', () => {
    return expectSaga(handleWearablesRequest, wearablesRequest({ collectionIds: ['some-other-collection'] }, context))
      .put(wearablesFailure(context, WRONG_FILTERS_ERROR))
      .provide([
        [select(baseCatalogsLoaded), true],
      ])
      .run()
  })

  it('When request fails, then the failure is informed', () => {
    const errorMessage = 'Something failed'
    return expectSaga(handleWearablesFailure, wearablesFailure(context, errorMessage))
      .call(informRequestFailure, errorMessage, context)
      .provide([
        [call(ensureRenderer), true],
        [call(informRequestFailure, errorMessage, context), null],
      ])
      .run()
  })

})
