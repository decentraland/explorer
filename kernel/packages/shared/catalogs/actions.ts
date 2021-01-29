import { action } from 'typesafe-actions'
import { Catalog, Wearable, WearableId } from './types'

// Wearables catalog

export const CATALOG_REQUEST = 'Catalog Request'
export const catalogRequest = (wearableIds: WearableId[]) => action(CATALOG_REQUEST, { wearableIds })
export type CatalogRequestAction = ReturnType<typeof catalogRequest>

export const CATALOG_SUCCESS = 'Catalog Success'
export const catalogSuccess = (wearables: Wearable[]) => action(CATALOG_SUCCESS, { wearables })
export type CatalogSuccessAction = ReturnType<typeof catalogSuccess>

export const CATALOG_LOADED = 'Catalog Loaded'
export const catalogLoaded = (name: string, catalog: Catalog) => action(CATALOG_LOADED, { name, catalog })
export type CatalogLoadedAction = ReturnType<typeof catalogLoaded>
