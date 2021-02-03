import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import { fetchInventoryItemsByAddress, handleCatalogRequest, handleCatalogSuccess, handleInventoryRequest, handleInventorySuccess, sendInventory, sendWearablesCatalog } from 'shared/catalogs/sagas'
import { catalogRequest, catalogSuccess, inventoryFailure, inventoryRequest, inventorySuccess } from 'shared/catalogs/actions'
import { baseCatalogsLoaded, getExclusiveCatalog, getPlatformCatalog } from 'shared/catalogs/selectors'
import { ensureRenderer } from 'shared/renderer/sagas'
import { throwError } from 'redux-saga-test-plan/providers'

const wearableId1 = 'WearableId1'
const wearable1 = { id: wearableId1 } as any
const userId = 'userId'

describe('Catalog Saga', () => {

  it('When catalog is requested for a wearable, then it is returned successfully', () => {
    return expectSaga(handleCatalogRequest, catalogRequest([wearableId1]))
      .put(catalogSuccess([wearable1]))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getPlatformCatalog), { }],
        [select(getExclusiveCatalog), { [wearableId1]: wearable1 }],
      ])
      .run()
  })

  it('When catalog fetch is successful, then it is sent to the renderer', () => {
    const wearables = [wearable1]
    return expectSaga(handleCatalogSuccess, catalogSuccess(wearables))
      .call(sendWearablesCatalog, wearables)
      .provide([
        [call(ensureRenderer), true],
        [call(sendWearablesCatalog, wearables), null],
      ])
      .run()
  })

  it('When inventory is requested for a wearable, then it is returned successfully', () => {
    const wearables = [wearable1]
    return expectSaga(handleInventoryRequest, inventoryRequest(userId))
      .put(inventorySuccess(userId, wearables))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getExclusiveCatalog), { [wearableId1]: wearable1 }],
        [call(fetchInventoryItemsByAddress, userId), Promise.resolve([wearableId1])],
      ])
      .run()
  })

  it('When inventory fails to load, then the request fails', () => {
    const error = new Error('Something failed')
    return expectSaga(handleInventoryRequest, inventoryRequest(userId))
      .put(inventoryFailure(userId, error))
      .provide([
        [select(baseCatalogsLoaded), true],
        [select(getExclusiveCatalog), { [wearableId1]: wearable1 }],
        [call(fetchInventoryItemsByAddress, userId), throwError(error)],
      ])
      .run()
  })

  it('When inventory fetch is successful, then it is sent to the renderer', () => {
    const wearables = [wearable1]
    const wearableIds = [wearableId1]
    return expectSaga(handleInventorySuccess, inventorySuccess(userId, wearables))
      .call(sendWearablesCatalog, wearables)
      .call(sendInventory, wearableIds)
      .provide([
        [call(ensureRenderer), true],
        [call(sendWearablesCatalog, wearables), null],
        [call(sendInventory, wearableIds), null],
      ])
      .run()
  })

})
