import { RarityEnum } from '../airdrops/interface'

export type Catalog = PartialWearableV2[]

export type Collection = { id: string; wearables: Wearable }

export type Wearable = {
  id: WearableId
  type: 'wearable'
  category: string
  baseUrl: string
  baseUrlBundles: string
  description: string
  tags: string[]
  hides?: string[]
  replaces?: string[]
  rarity: RarityEnum
  representations: BodyShapeRepresentation[]
  i18n: { code: string; text: string }[]
  thumbnail: string
}

export type WearableV2 = {
  id: string
  rarity: string
  i18n: { code: string; text: string }[]
  thumbnail: string
  description: string
  data: {
    category: string
    tags: string[]
    hides?: string[]
    replaces?: string[]
    representations: BodyShapeRepresentationV2[]
  }
  baseUrl: string
  baseUrlBundles: string
}

export type BodyShapeRepresentationV2 = {
  bodyShapes: string[]
  mainFile: string
  overrideHides?: string[]
  overrideReplaces?: string[]
  contents: KeyAndHash[]
}

type KeyAndHash = {
  key: string
  hash: string
}

export type PartialWearableV2 = PartialBy<Omit<WearableV2, 'baseUrlBundles'>, 'baseUrl'>
type PartialBy<T, K extends keyof T> = Omit<T, K> & Partial<Pick<T, K>>

export type BodyShapeRepresentation = {
  bodyShapes: string[]
  mainFile: string
  overrideHides?: string[]
  overrideReplaces?: string[]
  contents: FileAndHash[]
}

type FileAndHash = {
  file: string
  hash: string
}

export type WearableId = string

export type ColorString = string

export type CatalogState = {
  catalogs: {
    [key: string]: { id: string; status: 'error' | 'ok'; data?: Record<WearableId, PartialWearableV2>; error?: any }
  }
}

export type RootCatalogState = {
  catalogs: CatalogState
}

export type WearablesRequestFilters = {
  ownedByUser?: string
  wearableIds?: WearableId[]
  collectionIds?: string[]
}
