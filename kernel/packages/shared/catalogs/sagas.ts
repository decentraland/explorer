import { call, put, select, take, takeEvery, takeLatest } from 'redux-saga/effects'

import {
  getServerConfigurations,
  getWearablesSafeURL,
  PIN_CATALYST,
  WSS_ENABLED,
  TEST_WEARABLES_OVERRIDE,
  ALL_WEARABLES
} from 'config'

import defaultLogger from 'shared/logger'
import { RENDERER_INITIALIZED } from 'shared/renderer/types'
import {
  CatalogFailureAction,
  catalogLoaded,
  CatalogRequestAction,
  catalogSuccess,
  CatalogSuccessAction,
  CATALOG_FAILURE,
  CATALOG_LOADED,
  CATALOG_REQUEST,
  CATALOG_SUCCESS,
  InventoryFailure,
  inventoryFailure,
  InventoryRequest,
  InventorySuccess,
  inventorySuccess,
  INVENTORY_FAILURE,
  INVENTORY_REQUEST,
  INVENTORY_SUCCESS
} from './actions'
import { baseCatalogsLoaded, getExclusiveCatalog, getPlatformCatalog } from './selectors'
import { Catalog, Wearable, Collection, WearableId } from './types'
import { WORLD_EXPLORER } from '../../config/index'
import { getResourcesURL } from '../location'
import { UnityInterfaceContainer } from 'unity-interface/dcl'
import { StoreContainer } from '../store/rootTypes'
import { retrieve, store } from 'shared/cache'
import { ensureRealmInitialized } from 'shared/dao/sagas'
import { ensureRenderer } from 'shared/renderer/sagas'

declare const globalThis: Window & UnityInterfaceContainer & StoreContainer

/**
 * This saga handles wearable definition fetching.
 *
 * When the renderer detects a new wearable, but it doesn't know its definition, then it will create a catalog request.
 *
 * This request will include the ids of the unknown wearables. We will then find the appropriate definition, and return it to the renderer.
 *
 */
export function* catalogsSaga(): any {
  yield takeEvery(RENDERER_INITIALIZED, initialLoad)

  yield takeLatest(CATALOG_REQUEST, handleCatalogRequest)
  yield takeLatest(CATALOG_SUCCESS, handleCatalogSuccess)
  yield takeLatest(CATALOG_FAILURE, handleCatalogFailure)
  yield takeLatest(INVENTORY_REQUEST, handleInventoryRequest)
  yield takeLatest(INVENTORY_SUCCESS, handleInventorySuccess)
  yield takeLatest(INVENTORY_FAILURE, handleInventoryFailure)
}

function overrideBaseUrl(wearable: Wearable) {
  if (!TEST_WEARABLES_OVERRIDE) {
    return {
      ...wearable,
      baseUrl: getWearablesSafeURL() + '/contents/',
      baseUrlBundles: PIN_CATALYST ? '' : getServerConfigurations().contentAsBundle + '/'
    }
  } else {
    return wearable
  }
}

function* initialLoad() {
  yield call(ensureRealmInitialized)

  if (WORLD_EXPLORER) {
    try {
      const catalogUrl = getServerConfigurations().avatar.catalog

      let collections: Collection[] | undefined
      if (globalThis.location.search.match(/TEST_WEARABLES/)) {
        collections = [{ id: 'all', wearables: (yield call(fetchCatalog, catalogUrl))[0] }]
      } else {
        const cached = yield retrieve('catalog')

        if (cached) {
          const version = yield headCatalog(catalogUrl)
          if (cached.version === version) {
            collections = cached.data
          }
        }

        if (!collections) {
          const response = yield call(fetchCatalog, catalogUrl)
          collections = response[0]

          const version = response[1]
          if (version) {
            yield store('catalog', { version, data: response[0] })
          }
        }
      }
      const catalog: Wearable[] = collections!
        .reduce((flatten, collection) => flatten.concat(collection.wearables), [] as Wearable[])
        .filter((wearable) => !!wearable)
        .map(overrideBaseUrl)
      const baseAvatars = catalog.filter((_: Wearable) => _.tags && !_.tags.includes('exclusive'))
      const baseExclusive = catalog.filter((_: Wearable) => _.tags && _.tags.includes('exclusive'))
      yield put(catalogLoaded('base-avatars', baseAvatars))
      yield put(catalogLoaded('base-exclusive', baseExclusive))
    } catch (error) {
      defaultLogger.error('[FATAL]: Could not load catalog!', error)
    }
  } else {
    let baseCatalog = []
    try {
      const catalogPath = '/default-profile/basecatalog.json'
      const response = yield fetch(getResourcesURL() + catalogPath)
      baseCatalog = yield response.json()

      if (WSS_ENABLED) {
        for (let item of baseCatalog) {
          item.baseUrl = `http://localhost:8000${item.baseUrl}`
        }
      }
    } catch (e) {
      defaultLogger.warn(`Could not load base catalog`)
    }
    yield put(catalogLoaded('base-avatars', baseCatalog))
    yield put(catalogLoaded('base-exclusive', []))
  }
}

export function* handleCatalogRequest(action: CatalogRequestAction) {
  const { wearableIds } = action.payload

  yield call(ensureBaseCatalogs)

  const platformCatalog = yield select(getPlatformCatalog)
  const exclusiveCatalog = yield select(getExclusiveCatalog)

  const wearables: Wearable[] = wearableIds
    .map((wearableId) =>
      wearableId.includes(`base-avatars`) ? platformCatalog[wearableId] : exclusiveCatalog[wearableId]
    )
    .filter((wearable) => !!wearable)
  yield put(catalogSuccess(wearables))
}

export function* handleCatalogSuccess(action: CatalogSuccessAction) {
  const { wearables } = action.payload

  yield call(ensureRenderer)

  yield call(sendWearablesCatalog, wearables)
}

function* handleCatalogFailure(action: CatalogFailureAction) {
  const { wearableIds, error } = action.payload

  yield call(ensureRenderer)

  defaultLogger.error(`Failed to fetch wearables ${wearableIds.join(',')}`, error)

  // TODO: Decide what else to do on failure
}

async function headCatalog(url: string) {
  const request = await fetch(url, { method: 'HEAD' })
  if (!request.ok) {
    throw new Error('Catalog not found')
  }
  return request.headers.get('etag')
}

async function fetchCatalog(url: string) {
  const request = await fetch(url)
  if (!request.ok) {
    throw new Error('Catalog not found')
  }
  const etag = request.headers.get('etag')
  return [await request.json(), etag]
}

export function sendWearablesCatalog(catalog: Catalog) {
  globalThis.unityInterface.AddWearablesToCatalog(catalog)
}

export function* ensureBaseCatalogs() {
  while (!(yield select(baseCatalogsLoaded))) {
    yield take(CATALOG_LOADED)
  }
}

export function* handleInventoryRequest(action: InventoryRequest) {
  const { userId } = action.payload
  try {
    yield call(ensureBaseCatalogs)

    const exclusiveCatalog = yield select(getExclusiveCatalog)
    let inventoryItems: Wearable[]
    if (ALL_WEARABLES) {
      inventoryItems = Object.values(exclusiveCatalog)
    } else {
      const inventoryItemIds: WearableId[] = yield call(fetchInventoryItemsByAddress, userId)
      inventoryItems = inventoryItemIds.map((id) => exclusiveCatalog[id]).filter((wearable) => !!wearable)
    }
    yield put(inventorySuccess(userId, inventoryItems))
  } catch (error) {
    yield put(inventoryFailure(userId, error))
  }
}

export function* handleInventorySuccess(action: InventorySuccess) {
  const { inventory: wearables } = action.payload
  const wearableIds = wearables.map(({ id }) => id)

  yield call(ensureRenderer)

  // We are filling the catalog & updating the inventory in one operation. The idea is to avoid an extra step in displaying the whole inventory
  yield call(sendWearablesCatalog, wearables)
  yield call(sendInventory, wearableIds)
}

function* handleInventoryFailure(action: InventoryFailure) {
  const { userId, error } = action.payload

  yield call(ensureRenderer)

  defaultLogger.error(`Failed to fetch inventory for user ${userId}`, error)

  // TODO: Decide what else to do on failure
}

export async function fetchInventoryItemsByAddress(address: string): Promise<WearableId[]> {
  if (!WORLD_EXPLORER) {
    return []
  }
  const result = await fetch(`${getServerConfigurations().wearablesApi}/addresses/${address}/wearables?fields=id`)
  if (!result.ok) {
    throw new Error('Unable to fetch inventory for address ' + address)
  }
  const inventory: { id: string }[] = await result.json()

  return inventory.map((wearable) => wearable.id)
}

export function sendInventory(inventory: WearableId[]) {
  globalThis.unityInterface.SetInventory(inventory)
}
