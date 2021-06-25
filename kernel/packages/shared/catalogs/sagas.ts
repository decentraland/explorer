import { call, put, select, take, takeEvery } from 'redux-saga/effects'

import {
  getServerConfigurations,
  getWearablesSafeURL,
  PIN_CATALYST,
  WSS_ENABLED,
  TEST_WEARABLES_OVERRIDE,
  ALL_WEARABLES,
  WITH_FIXED_COLLECTIONS
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
import {
  Wearable,
  Collection,
  WearableId,
  WearablesRequestFilters,
  WearableV2,
  BodyShapeRepresentationV2,
  PartialWearableV2,
  UnpublishedWearable
} from './types'
import { WORLD_EXPLORER } from '../../config/index'
import { getResourcesURL } from '../location'
import { RendererInterfaces } from 'unity-interface/dcl'
import { StoreContainer } from '../store/rootTypes'
import { retrieve, store } from 'shared/cache'
import { ensureRealmInitialized } from 'shared/dao/sagas'
import { ensureRenderer } from 'shared/renderer/sagas'
import { isFeatureEnabled } from 'shared/meta/selectors'
import { FeatureFlags } from 'shared/meta/types'
import { CatalystClient, OwnedWearablesWithDefinition } from 'dcl-catalyst-client'
import { fetchJson } from 'dcl-catalyst-commons'
import { getCatalystServer, getFetchContentServer } from 'shared/dao/selectors'
import {
  BASE_BUILDER_SERVER_URL,
  BASE_DOWNLOAD_URL,
  BuilderServerAPIManager
} from 'shared/apis/SceneStateStorageController/BuilderServerAPIManager'
import { getCurrentIdentity } from 'shared/session/selectors'
import { userAuthentified } from 'shared/session'

declare const globalThis: Window & RendererInterfaces & StoreContainer
export const BASE_AVATARS_COLLECTION_ID = 'urn:decentraland:off-chain:base-avatars'
export const WRONG_FILTERS_ERROR = `You must set one and only one filter for V1. Also, the only collection id allowed is '${BASE_AVATARS_COLLECTION_ID}'`

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

function overrideBaseUrl(wearable: PartialWearableV2): PartialWearableV2 {
  if (!TEST_WEARABLES_OVERRIDE) {
    return {
      ...wearable,
      baseUrl: getWearablesSafeURL() + '/contents/'
    }
  } else {
    return wearable
  }
}

function* initialLoad() {
  yield call(ensureRealmInitialized)

  const shouldUseV2 = yield select(isFeatureEnabled, FeatureFlags.WEARABLES_V2, false)

  if (WORLD_EXPLORER && !shouldUseV2) {
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
      const catalog: PartialWearableV2[] = collections!
        .reduce((flatten, collection) => flatten.concat(collection.wearables), [] as Wearable[])
        .filter((wearable) => !!wearable)
        .map(mapV1WearableIntoV2)
        .map(overrideBaseUrl)
      const baseAvatars = catalog.filter((_: PartialWearableV2) => _.data.tags && !_.data.tags.includes('exclusive'))
      const baseExclusive = catalog.filter((_: PartialWearableV2) => _.data.tags && _.data.tags.includes('exclusive'))
      yield put(catalogLoaded('base-avatars', baseAvatars))
      yield put(catalogLoaded('base-exclusive', baseExclusive))
    } catch (error) {
      defaultLogger.error('[FATAL]: Could not load catalog!', error)
    }
  } else if (!WORLD_EXPLORER) {
    let baseCatalog = []
    try {
      const catalogPath = '/default-profile/basecatalog.json'
      const response = yield fetch(getResourcesURL() + catalogPath)
      baseCatalog = yield response.json()
      baseCatalog = baseCatalog.map(mapV1WearableIntoV2)

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
      const downloadUrl = yield select(getFetchContentServer)

      const response: PartialWearableV2[] = shouldUseV2
        ? yield call(fetchWearablesV2, filters)
        : yield call(fetchWearablesV1, filters)

      const v2Wearables: WearableV2[] = response.map((wearable) => ({
        ...wearable,
        baseUrl: wearable.baseUrl ?? downloadUrl + '/contents/',
        baseUrlBundles: PIN_CATALYST ? '' : getServerConfigurations().contentAsBundle + '/'
      }))

      yield put(wearablesSuccess(v2Wearables, context))
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
    if (WITH_FIXED_COLLECTIONS) {
      // The WITH_FIXED_COLLECTIONS config can only be used in zone. However, we want to be able to use prod collections for testing.
      // That's why we are also querying a prod catalyst for the given collections
      const collectionIds: string[] = WITH_FIXED_COLLECTIONS.split(',')

      // Fetch published collections
      const urnCollections = collectionIds.filter((collectionId) => collectionId.startsWith('urn'))
      if (urnCollections.length > 0) {
        const orgClient: CatalystClient = yield CatalystClient.connectedToCatalystIn('mainnet', 'EXPLORER')
        const zoneWearables = yield client.fetchWearables({ collectionIds: urnCollections })
        const orgWearables = yield orgClient.fetchWearables({ collectionIds: urnCollections })
        result.push(...zoneWearables, ...orgWearables)
      }

      // Fetch unpublished collections from builder server
      const uuidCollections = collectionIds.filter((collectionId) => !collectionId.startsWith('urn'))
      if (uuidCollections.length > 0) {
        yield userAuthentified()
        const identity = yield select(getCurrentIdentity)
        for (const collectionUuid of uuidCollections) {
          const path = `collections/${collectionUuid}/items`
          const headers = BuilderServerAPIManager.authorize(identity, 'get', `/${path}`)
          const collection: { data: UnpublishedWearable[] } = yield fetchJson(`${BASE_BUILDER_SERVER_URL}${path}`, {
            headers
          })
          const v2Wearables = collection.data.map((wearable) => mapUnpublishedWearableIntoCatalystWearable(wearable))
          result.push(...v2Wearables)
        }
      }
    } else {
      const ownedWearables: OwnedWearablesWithDefinition[] = yield call(
        fetchOwnedWearables,
        filters.ownedByUser,
        client
      )
      for (const { amount, definition } of ownedWearables) {
        if (definition) {
          for (let i = 0; i < amount; i++) {
            result.push(definition)
          }
        }
      }
    }
  } else {
    const wearables = yield call(fetchWearablesByFilters, filters, client)
    result.push(...wearables)
  }

  return result.map(mapCatalystWearableIntoV2)
}

function fetchOwnedWearables(ethAddress: string, client: CatalystClient) {
  return client.fetchOwnedWearables(ethAddress, true)
}

async function fetchWearablesByFilters(filters: WearablesRequestFilters, client: CatalystClient) {
  return client.fetchWearables(filters)
}

/**
 * We are now mapping wearables that were fetched from the builder server into the same format that is returned by the catalysts
 */
function mapUnpublishedWearableIntoCatalystWearable(wearable: UnpublishedWearable): any {
  const { id, rarity, name, thumbnail, description, data, contents: contentToHash } = wearable
  return {
    id,
    rarity,
    i18n: [{ code: 'en', text: name }],
    thumbnail: `${BASE_DOWNLOAD_URL}/${contentToHash[thumbnail]}`,
    description,
    data: {
      ...data,
      representations: data.representations.map(({ contents, ...other }) => ({
        ...other,
        contents: contents.map((key) => ({
          key,
          url: `${BASE_DOWNLOAD_URL}/${contentToHash[key]}`
        }))
      }))
    }
  }
}

function mapV1WearableIntoV2(wearable: Wearable): PartialWearableV2 {
  const {
    category,
    tags,
    hides,
    replaces,
    representations,
    id,
    rarity,
    i18n,
    thumbnail,
    baseUrl,
    description
  } = wearable
  return {
    id: mapLegacyIdToUrn(id),
    rarity,
    i18n,
    thumbnail,
    description,
    data: {
      category,
      tags,
      hides,
      replaces,
      representations: representations.map(({ bodyShapes, contents, ...other }) => ({
        ...other,
        bodyShapes: mapLegacyIdsToUrn(bodyShapes),
        contents: contents.map(({ file, hash }) => ({ key: file, hash }))
      }))
    },
    baseUrl
  }
}

function mapCatalystRepresentationIntoV2(representation: any): BodyShapeRepresentationV2 {
  const { contents, ...other } = representation

  const newContents = contents.map(({ key, url }: { key: string; url: string }) => ({
    key,
    hash: url.substring(url.lastIndexOf('/') + 1)
  }))
  return {
    ...other,
    contents: newContents
  }
}

function mapCatalystWearableIntoV2(v2Wearable: any): PartialWearableV2 {
  const { id, data, rarity, i18n, thumbnail, description } = v2Wearable
  const { category, tags, hides, replaces, representations } = data
  const newRepresentations: BodyShapeRepresentationV2[] = representations.map(mapCatalystRepresentationIntoV2)
  const index = thumbnail.lastIndexOf('/')
  const newThumbnail = thumbnail.substring(index + 1)
  const baseUrl = thumbnail.substring(0, index + 1)

  return {
    id,
    rarity,
    i18n,
    thumbnail: newThumbnail,
    description,
    data: {
      category,
      tags,
      hides,
      replaces,
      representations: newRepresentations
    },
    baseUrl
  }
}

export function mapLegacyIdsToUrn(wearableIds: WearableId[]): WearableId[] {
  return wearableIds.map(mapLegacyIdToUrn)
}

export function mapLegacyIdToUrn(wearableId: WearableId): WearableId {
  if (!wearableId.startsWith('dcl://')) {
    return wearableId
  }
  if (wearableId.startsWith('dcl://base-avatars')) {
    const name = wearableId.substring(wearableId.lastIndexOf('/') + 1)
    return `urn:decentraland:off-chain:base-avatars:${name}`
  } else {
    const [collectionName, wearableName] = wearableId.replace('dcl://', '').split('/')
    return `urn:decentraland:ethereum:collections-v1:${collectionName}:${wearableName}`
  }
}

function* fetchWearablesV1(filters: WearablesRequestFilters) {
  yield call(ensureBaseCatalogs)

  const platformCatalog = yield select(getPlatformCatalog)
  const exclusiveCatalog = yield select(getExclusiveCatalog)

  let response: PartialWearableV2[]
  if (filters.wearableIds) {
    // Filtering by ids
    response = filters.wearableIds
      .map((wearableId) =>
        wearableId === 'urn:decentraland:off-chain:base-avatars:SchoolShoes'
          ? 'urn:decentraland:off-chain:base-avatars:Moccasin'
          : wearableId
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
    if (filters.collectionIds.some((id) => id !== BASE_AVATARS_COLLECTION_ID)) {
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

export function sendWearablesCatalog(wearables: WearableV2[], context: string | undefined) {
  globalThis.unityInterface.AddWearablesToCatalog(wearables, context)
}

export function* ensureBaseCatalogs() {
  const shouldUseV2: boolean =
    WORLD_EXPLORER && isFeatureEnabled(globalThis.globalStore.getState(), FeatureFlags.WEARABLES_V2, false)

  while (!shouldUseV2 && !(yield select(baseCatalogsLoaded))) {
    yield take(CATALOG_LOADED)
  }
}

async function fetchInventoryItemsByAddress(address: string): Promise<WearableId[]> {
  if (!WORLD_EXPLORER) {
    return []
  }
  const result = await fetch(`${getServerConfigurations().wearablesApi}/addresses/${address}/wearables?fields=id`)
  if (!result.ok) {
    throw new Error('Unable to fetch inventory for address ' + address)
  }
  const inventory: { id: string }[] = await result.json()

  return mapLegacyIdsToUrn(inventory.map((wearable) => wearable.id))
}
