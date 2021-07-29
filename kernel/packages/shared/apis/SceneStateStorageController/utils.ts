import { CLASS_ID } from 'decentraland-ecs'
import { SceneSourcePlacement } from 'shared/types'

/**
 * We are converting from numeric ids to a more human readable format. It might make sense to change this in the future,
 * but until this feature is stable enough, it's better to store it in a way that it is easy to debug.
 */

const HUMAN_READABLE_TO_ID: Map<string, number> = new Map([
  ['Transform', CLASS_ID.TRANSFORM],
  ['GLTFShape', CLASS_ID.GLTF_SHAPE],
  ['NFTShape', CLASS_ID.NFT_SHAPE],
  ['Name', CLASS_ID.NAME],
  ['LockedOnEdit', CLASS_ID.LOCKED_ON_EDIT],
  ['VisibleOnEdit', CLASS_ID.VISIBLE_ON_EDIT],
  ['Script', CLASS_ID.SMART_ITEM]
])

export function toHumanReadableType(type: number): string {
  const humanReadableType = Array.from(HUMAN_READABLE_TO_ID.entries())
    .filter(([, componentId]) => componentId === type)
    .map(([type]) => type)[0]
  if (!humanReadableType) {
    throw new Error(`Unknown type ${type}`)
  }
  return humanReadableType
}

export function fromHumanReadableType(humanReadableType: string): number {
  const type = HUMAN_READABLE_TO_ID.get(humanReadableType)
  if (!type) {
    throw new Error(`Unknown human readable type ${humanReadableType}`)
  }
  return type
}

export function getLayoutFromParcels(parcels: string[]): SceneSourcePlacement['layout'] {
  let rows = 1
  let cols = 1

  if (parcels.length > 1) {
    rows = [...new Set(parcels.map((parcel) => parcel.split(',')[1]))].length
    cols = [...new Set(parcels.map((parcel) => parcel.split(',')[0]))].length
  }
  return { cols, rows }
}
