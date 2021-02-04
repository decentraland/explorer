import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import { fetchInventoryItemsByAddress, handleWearablesRequest, handleWearablesSuccess, sendWearablesCatalog, WRONG_FILTERS_ERROR } from 'shared/catalogs/sagas'
import { wearablesFailure, wearablesRequest, wearablesSuccess } from 'shared/catalogs/actions'
import { baseCatalogsLoaded, getExclusiveCatalog, getPlatformCatalog } from 'shared/catalogs/selectors'
import { ensureRenderer } from 'shared/renderer/sagas'
import { throwError } from 'redux-saga-test-plan/providers'
import { getUserId } from 'shared/session/selectors'

const wearableId1 = 'WearableId1'
const wearable1 = { id: wearableId1 } as any
const userId = 'userId'
const context = 'someContext'

describe('Wearables Saga', () => {

  it('When a wearable is requested by id, then it is returned successfully', () => {
    return expectSaga(handleWearablesRequest, wearablesRequest({ wearableIds: [wearableId1], ownedByUser: false }, context))
      .put(wearablesSuccess([wearable1], context))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getPlatformCatalog), { }],
        [select(getExclusiveCatalog), { [wearableId1]: wearable1 }],
      ])
      .run()
  })

  it('When all owned wearables are requested, then they are returned successfully', () => {
    const wearables = [wearable1]
    return expectSaga(handleWearablesRequest, wearablesRequest({ ownedByUser: true }, context))
      .put(wearablesSuccess(wearables, context))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getPlatformCatalog), { }],
        [select(getExclusiveCatalog), { [wearableId1]: wearable1 }],
        [select(getUserId), userId],
        [call(fetchInventoryItemsByAddress, userId), Promise.resolve([wearableId1])],
      ])
      .run()
  })

  it('When base avatars are requested, then they are returned successfully', () => {
    const baseWearables = [wearable1]
    return expectSaga(handleWearablesRequest, wearablesRequest({ collectionNames: ['base-avatars'], ownedByUser: false }, context))
      .put(wearablesSuccess(baseWearables, context))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getPlatformCatalog), { [wearableId1]: wearable1 }],
        [select(getExclusiveCatalog), { }],
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

  it('When wearables fetch fails to load, then the request fails', () => {
    const error = new Error('Something failed')
    return expectSaga(handleWearablesRequest, wearablesRequest({ ownedByUser: true }, context))
      .put(wearablesFailure(context, error))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getPlatformCatalog), { }],
        [select(getExclusiveCatalog), { }],
        [select(getUserId), userId],
        [call(fetchInventoryItemsByAddress, userId), throwError(error)],
      ])
      .run()
  })

  it('When more than one filter is set, then the request fails', () => {
    return expectSaga(handleWearablesRequest, wearablesRequest({ wearableIds: ['some-id'], ownedByUser: true }, context))
      .put(wearablesFailure(context, WRONG_FILTERS_ERROR))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getUserId), userId],
        [select(getPlatformCatalog), { }],
        [select(getExclusiveCatalog), { }],
      ])
      .run()
  })

  it('When collection name is not base-avatars, then the request fails', () => {
    return expectSaga(handleWearablesRequest, wearablesRequest({ collectionNames: ['some-other-collection'], ownedByUser: false }, context))
      .put(wearablesFailure(context, WRONG_FILTERS_ERROR))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getUserId), userId],
      ])
      .run()
  })

})
