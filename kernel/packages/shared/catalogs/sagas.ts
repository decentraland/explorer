import { call, put, select, take, takeEvery } from 'redux-saga/effects'

import { WSS_ENABLED, WITH_FIXED_COLLECTIONS, getAssetBundlesBaseUrl } from 'config'

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
import { baseCatalogsLoaded, getPlatformCatalog } from './selectors'
import {
  WearablesRequestFilters,
  WearableV2,
  BodyShapeRepresentationV2,
  PartialWearableV2,
  UnpublishedWearable
} from './types'
import { WORLD_EXPLORER } from '../../config/index'
import { getResourcesURL } from '../location'
import { StoreContainer } from '../store/rootTypes'
import { ensureRealmInitialized } from 'shared/dao/sagas'
import { ensureRenderer } from 'shared/renderer/sagas'
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
import { unityInterface } from 'unity-interface/UnityInterface'

declare const globalThis: Window & StoreContainer
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

function* initialLoad() {
  yield call(ensureRealmInitialized)

  if (!WORLD_EXPLORER) {
    let baseCatalog = []
    try {
      const catalogPath = '/default-profile/basecatalog.json'
      const response = yield fetch(getResourcesURL(catalogPath))
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
      const shouldUseLocalCatalog = WORLD_EXPLORER
      const downloadUrl = yield select(getFetchContentServer)

      const response: PartialWearableV2[] = shouldUseLocalCatalog
        ? yield call(fetchWearablesFromCatalyst, filters)
        : yield call(fetchWearablesFromLocalCatalog, filters)

      const assetBundlesBaseUrl: string = getAssetBundlesBaseUrl() + '/'

      const v2Wearables: WearableV2[] = response.map((wearable) => ({
        ...wearable,
        baseUrl: wearable.baseUrl ?? downloadUrl + '/contents/',
        baseUrlBundles: assetBundlesBaseUrl
      }))

      yield put(wearablesSuccess(v2Wearables, context))
    } catch (error) {
      yield put(wearablesFailure(context, error.message))
    }
  } else {
    yield put(wearablesFailure(context, WRONG_FILTERS_ERROR))
  }
}

function* fetchWearablesFromCatalyst(filters: WearablesRequestFilters) {
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

function* fetchWearablesFromLocalCatalog(filters: WearablesRequestFilters) {
  yield call(ensureBaseCatalogs)

  const platformCatalog = yield select(getPlatformCatalog)

  let response: PartialWearableV2[]
  if (filters.wearableIds) {
    // Filtering by ids
    response = filters.wearableIds.map((wearableId) => platformCatalog[wearableId]).filter((wearable) => !!wearable)
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

export function informRequestFailure(error: string, context: string | undefined) {
  unityInterface.WearablesRequestFailed(error, context)
}

export function sendWearablesCatalog(wearables: WearableV2[], context: string | undefined) {
  unityInterface.AddWearablesToCatalog(wearables, context)
}

export function* ensureBaseCatalogs() {
  while (!WORLD_EXPLORER && !(yield select(baseCatalogsLoaded))) {
    yield take(CATALOG_LOADED)
  }
}
