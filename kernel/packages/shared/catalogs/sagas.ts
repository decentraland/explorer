import { call, put, select, take, takeEvery, takeLatest } from 'redux-saga/effects'

import {
  getServerConfigurations,
  getWearablesSafeURL,
  PIN_CATALYST,
  WSS_ENABLED,
  TEST_WEARABLES_OVERRIDE
} from 'config'

import defaultLogger from 'shared/logger'
import { isInitialized } from 'shared/renderer/selectors'
import { RENDERER_INITIALIZED } from 'shared/renderer/types'
import {
  catalogLoaded,
  CatalogRequestAction,
  catalogSuccess,
  CatalogSuccessAction,
  CATALOG_LOADED,
  CATALOG_REQUEST,
  CATALOG_SUCCESS
} from './actions'
import { baseCatalogsLoaded, getExclusiveCatalog, getPlatformCatalog } from './selectors'
import { Catalog, Wearable, Collection } from './types'
import { WORLD_EXPLORER } from '../../config/index'
import { getResourcesURL } from '../location'
import { UnityInterfaceContainer } from 'unity-interface/dcl'
import { StoreContainer } from '../store/rootTypes'
import { retrieve, store } from 'shared/cache'
import { ensureRealmInitialized } from 'shared/dao/sagas'
import { ensureRenderer } from 'shared/renderer/sagas'

declare const globalThis: Window & UnityInterfaceContainer & StoreContainer

/**
 * This saga handles both passports and assets required for the renderer to show the
 * users' inventory and avatar editor.
 *
 * When the renderer is initialized, it will fetch the asset catalog and submit it to the renderer.
 *
 * Whenever a passport is requested, it will fetch it and store it locally (see also: `selectors.ts`)
 *
 * If a user avatar was not found, it will create a random passport (see: `handleRandomAsSuccess`)
 *
 * Lastly, we handle save requests by submitting both to the avatar legacy server as well as to the profile server.
 *
 * It's *very* important for the renderer to never receive a passport with items that have not been loaded into the catalog.
 */
export function* catalogsSaga(): any {
  yield takeEvery(RENDERER_INITIALIZED, initialLoad)

  yield takeLatest(CATALOG_REQUEST, handleCatalogRequest)
  yield takeLatest(CATALOG_SUCCESS, handleCatalogSuccess)
}

function overrideBaseUrl(wearable: Wearable) {
  if (!TEST_WEARABLES_OVERRIDE) {
    return {
      ...wearable,
      baseUrl: getWearablesSafeURL() + '/contents/',
      baseUrlBundles: PIN_CATALYST ? '' : getServerConfigurations().contentAsBundle + '/'
    }
  } else {
    return wearable ?? {}
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
      const catalog = collections!
        .reduce((flatten, collection) => flatten.concat(collection.wearables), [] as Wearable[])
        .map(overrideBaseUrl)
      const baseAvatars = catalog.filter((_: Wearable) => _.tags && !_.tags.includes('exclusive'))
      const baseExclusive = catalog.filter((_: Wearable) => _.tags && _.tags.includes('exclusive'))
      if (!(yield select(isInitialized))) {
        yield take(RENDERER_INITIALIZED)
      }
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

function* handleCatalogRequest(action: CatalogRequestAction) {
  const { wearableIds } = action.payload

  yield call(ensureBaseCatalogs)

  const platformCatalog = yield select(getPlatformCatalog)
  const exclusiveCatalog = yield select(getExclusiveCatalog)

  const wearables: Wearable[] = wearableIds
    .map((wearableId) =>
      wearableId.startsWith(`dcl://base-avatars`) ? platformCatalog[wearableId] : exclusiveCatalog[wearableId]
    )
    .filter((wearable) => !!wearable)
  yield put(catalogSuccess(wearables))
}

function* handleCatalogSuccess(action: CatalogSuccessAction) {
  const { wearables } = action.payload

  yield call(ensureRenderer)

  yield call(sendWearablesCatalog, wearables)
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

function sendWearablesCatalog(catalog: Catalog) {
  globalThis.unityInterface.AddWearablesToCatalog(catalog)
}

export function* ensureBaseCatalogs() {
  while (!(yield select(baseCatalogsLoaded))) {
    yield take(CATALOG_LOADED)
  }
}
