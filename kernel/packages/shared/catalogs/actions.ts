import { action } from 'typesafe-actions'
import { Catalog, Wearable, WearableId } from './types'

// Wearables catalog

export const CATALOG_REQUEST = 'Catalog Request'
export const catalogRequest = (wearableIds: WearableId[]) => action(CATALOG_REQUEST, { wearableIds })
export type CatalogRequestAction = ReturnType<typeof catalogRequest>

export const CATALOG_SUCCESS = 'Catalog Success'
export const catalogSuccess = (wearables: Wearable[]) => action(CATALOG_SUCCESS, { wearables })
export type CatalogSuccessAction = ReturnType<typeof catalogSuccess>

export const CATALOG_FAILURE = 'Catalog Failure'
export const catalogFailure = (wearableIds: WearableId[], error: any) => action(CATALOG_FAILURE, { wearableIds, error })
export type CatalogFailureAction = ReturnType<typeof catalogFailure>

// TODO: Remove after wearable migration to the content server (Nico C)
export const CATALOG_LOADED = 'Catalog Loaded'
export const catalogLoaded = (name: string, catalog: Catalog) => action(CATALOG_LOADED, { name, catalog })
export type CatalogLoadedAction = ReturnType<typeof catalogLoaded>

// Inventory

export const INVENTORY_REQUEST = '[Request] Inventory fetch'
export const inventoryRequest = (userId: string) => action(INVENTORY_REQUEST, { userId })
export type InventoryRequest = ReturnType<typeof inventoryRequest>

export const INVENTORY_SUCCESS = '[Success] Inventory fetch'
export const inventorySuccess = (userId: string, inventory: Wearable[]) =>
  action(INVENTORY_SUCCESS, { userId, inventory })
export type InventorySuccess = ReturnType<typeof inventorySuccess>

export const INVENTORY_FAILURE = '[Failure] Inventory fetch'
export const inventoryFailure = (userId: string, error: any) => action(INVENTORY_FAILURE, { userId, error })
export type InventoryFailure = ReturnType<typeof inventoryFailure>
