import { action } from 'typesafe-actions'
import { Catalog, Wearable, WearableId } from './types'

// Wearables catalog

export const ADD_CATALOG = 'Add Catalog'
export const addCatalog = (name: string, catalog: Catalog) => action(ADD_CATALOG, { name, catalog })
export type AddCatalogAction = ReturnType<typeof addCatalog>

export const CATALOG_REQUEST = 'Catalog Request'
export const catalogRequest = (wearableIds: WearableId[]) => action(CATALOG_REQUEST, { wearableIds })
export type CatalogRequestAction = ReturnType<typeof catalogRequest>

export const CATALOG_SUCCESS = 'Catalog Success'
export const catalogSuccess = (wearables: Wearable[]) => action(CATALOG_SUCCESS, { wearables })
export type CatalogSuccessAction = ReturnType<typeof catalogSuccess>

export const CATALOG_LOADED = 'Catalog Loaded'
export const catalogLoaded = (name: string) => action(CATALOG_LOADED, { name })
export type CatalogLoadedAction = ReturnType<typeof catalogLoaded>
