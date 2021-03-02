import { call, put, select, take, takeEvery } from 'redux-saga/effects'

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
  catalogLoaded,
  CATALOG_LOADED,
  WearablesFailure,
  wearablesFailure,
  WearablesRequest,
  WearablesSuccess,
  wearablesSuccess,
  WEARABLES_FAILURE,
  WEARABLES_REQUEST,
  WEARABLES_SUCCESS
} from './actions'
import { baseCatalogsLoaded, getExclusiveCatalog, getPlatformCatalog } from './selectors'
import { Catalog, Wearable, Collection, WearableId, WearablesRequestFilters, BodyShapeRepresentation } from './types'
import { WORLD_EXPLORER } from '../../config/index'
import { getResourcesURL } from '../location'
import { UnityInterfaceContainer } from 'unity-interface/dcl'
import { StoreContainer } from '../store/rootTypes'
import { retrieve, store } from 'shared/cache'
import { ensureRealmInitialized } from 'shared/dao/sagas'
import { ensureRenderer } from 'shared/renderer/sagas'
import { isFeatureEnabled } from 'shared/meta/selectors'
import { FeatureFlags } from 'shared/meta/types'
import { CatalystClient, OwnedWearablesWithDefinition } from 'dcl-catalyst-client'
import { parseUrn } from '@dcl/urn-resolver'
import { getCatalystServer } from 'shared/dao/selectors'

declare const globalThis: Window & UnityInterfaceContainer & StoreContainer
export const WRONG_FILTERS_ERROR =
  'You must set one and only one filter for V1. Also, the only collection name allowed is base-avatars'

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

  yield takeEvery(WEARABLES_REQUEST, handleWearablesRequest)
  yield takeEvery(WEARABLES_SUCCESS, handleWearablesSuccess)
  yield takeEvery(WEARABLES_FAILURE, handleWearablesFailure)
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

export function* handleWearablesRequest(action: WearablesRequest) {
  const { filters, context } = action.payload

  const valid = areFiltersValid(filters)
  if (valid) {
    try {
      const shouldUseV2 = WORLD_EXPLORER && (yield select(isFeatureEnabled, FeatureFlags.WEARABLES_V2, false))

      const response: Wearable[] = shouldUseV2
        ? yield call(fetchWearablesV2, filters)
        : yield call(fetchWearablesV1, filters)

      yield put(wearablesSuccess(response, context))
    } catch (error) {
      yield put(wearablesFailure(context, error.message))
    }
  } else {
    yield put(wearablesFailure(context, WRONG_FILTERS_ERROR))
  }
}

function* fetchWearablesV2(filters: WearablesRequestFilters) {
  const catalystUrl = yield select(getCatalystServer)
  const client: CatalystClient = new CatalystClient(catalystUrl, 'EXPLORER')

  const result: any[] = []
  if (filters.ownedByUser) {
    // TODO: What about ALL_WEARABLES?

    const ownedWearables: OwnedWearablesWithDefinition[] = yield call(fetchOwnedWearables, filters.ownedByUser, client)
    for (const { amount, definition } of ownedWearables) {
      for (let i = 0; i < amount; i++) {
        result.push(definition)
      }
    }
  } else {
    const wearables = yield call(fetchWearablesByFilters, filters, client)
    result.push(...wearables)
  }

  return result.map(mapV2WearableIntoV1).map(overrideBaseUrl)
}

function fetchOwnedWearables(ethAddress: string, client: CatalystClient) {
  return client.fetchOwnedWearables(ethAddress, true)
}

function fetchWearablesByFilters(filters: WearablesRequestFilters, client: CatalystClient) {
  // This is necessary because the renderer still has some hardcoded legacy ids. After the migration is successful and the flag is removed, the renderer can update the ids and we can remove this translation
  const newIds = !filters?.wearableIds ? undefined : filters.wearableIds.map(mapLegacyToUrn)
  return client.fetchWearables({
    ...filters,
    wearableIds: newIds
  })
}

function mapLegacyToUrn(id: WearableId): WearableId {
  if (!id.startsWith('dcl://base-avatars')) {
    return id
  }
  const name = id.substring(id.lastIndexOf('/') + 1)
  return `decentraland:off-chain:base-avatars:${name}`
}

function mapV2RepresentationIntoV1(representation: any): BodyShapeRepresentation {
  const { contents, ...other } = representation
  const newContents = contents.map(({ key, url }: { key: string; url: string }) => ({
    file: key,
    hash: url.substring(url.lastIndexOf('/') + 1)
  }))
  return {
    ...other,
    content: newContents
  }
}

/** We need to map the v2 wearable format into the v1 format, that is accepted by the renderer */
function mapV2WearableIntoV1(v2: any): Wearable {
  const { id, data, rarity } = v2
  const { category, tags, hides, replaces, representations } = data
  return {
    id,
    type: 'wearable',
    category,
    tags,
    hides,
    replaces,
    rarity,
    representations: representations.map(mapV2RepresentationIntoV1),
    baseUrl: '',
    baseUrlBundles: ''
  }
}

async function mapUrnToLegacyId(wearableIds: WearableId[]): Promise<WearableId[]> {
  const promises = wearableIds.map(async (wearableId) => {
    if (wearableId.startsWith('dcl://')) {
      return wearableId
    }

    try {
      const result = await parseUrn(wearableId)
      if (result?.type === 'off-chain') {
        return `dcl://${result.registry}/${result.id}`
      } else if (result?.type === 'blockchain-collection-v1') {
        return `dcl://${result.collectionName}/${result.id}`
      }
    } catch {}
  })

  const mappedIds = await Promise.all(promises)
  return mappedIds.filter((mappedId): mappedId is WearableId => !!mappedId)
}

function* fetchWearablesV1(filters: WearablesRequestFilters) {
  yield call(ensureBaseCatalogs)

  const platformCatalog = yield select(getPlatformCatalog)
  const exclusiveCatalog = yield select(getExclusiveCatalog)

  let response: Wearable[]
  if (filters.wearableIds) {
    // Filtering by ids
    const mappedFromUrns: WearableId[] = yield call(mapUrnToLegacyId, filters.wearableIds)
    response = mappedFromUrns
      .map((wearableId) =>
        wearableId === 'dcl://base-avatars/SchoolShoes' ? 'dcl://base-avatars/Moccasin' : wearableId
      )
      .map((wearableId) =>
        wearableId.includes(`base-avatars`) ? platformCatalog[wearableId] : exclusiveCatalog[wearableId]
      )
      .filter((wearable) => !!wearable)
  } else if (filters.ownedByUser) {
    // Only owned wearables
    if (ALL_WEARABLES) {
      response = Object.values(exclusiveCatalog)
    } else {
      const inventoryItemIds: WearableId[] = yield call(fetchInventoryItemsByAddress, filters.ownedByUser)
      response = inventoryItemIds.map((id) => exclusiveCatalog[id]).filter((wearable) => !!wearable)
    }
  } else if (filters.collectionIds) {
    // We assume that the only collection id used is base-avatars
    response = Object.values(platformCatalog)
  } else {
    throw new Error('Unknown filter')
  }
  return response
}

export function* handleWearablesSuccess(action: WearablesSuccess) {
  const { wearables, context } = action.payload

  yield call(ensureRenderer)
  yield call(sendWearablesCatalog, wearables, context)
}

export function* handleWearablesFailure(action: WearablesFailure) {
  const { context, error } = action.payload

  defaultLogger.error(`Failed to fetch wearables for context '${context}'`, error)

  yield call(ensureRenderer)
  yield call(informRequestFailure, error, context)
}

function areFiltersValid(filters: WearablesRequestFilters) {
  let filtersSet = 0
  let ok = true
  if (filters.collectionIds) {
    filtersSet += 1
    if (filters.collectionIds.some((name) => name !== 'base-avatars')) {
      ok = false
    }
  }

  if (filters.ownedByUser) {
    filtersSet += 1
  }

  if (filters.wearableIds) {
    filtersSet += 1
  }

  return filtersSet === 1 && ok
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

export function informRequestFailure(error: string, context: string | undefined) {
  globalThis.unityInterface.WearablesRequestFailed(error, context)
}

export function sendWearablesCatalog(catalog: Catalog, context: string | undefined) {
  globalThis.unityInterface.AddWearablesToCatalog(catalog, context)
}

export function* ensureBaseCatalogs() {
  while (!(yield select(baseCatalogsLoaded))) {
    yield take(CATALOG_LOADED)
  }
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
