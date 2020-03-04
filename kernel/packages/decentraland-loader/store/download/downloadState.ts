import { StringCoordinate, SceneContentServerEntity } from '../sceneInfo/types'
import { mapAdd } from '../utils/mapAdd'
import { mapRemove } from '../utils/mapRemove'
import { unique } from '../utils/unique'
import { setFilter } from '../utils/setFilter'
import { setRemove } from '../utils/setRemove'

export type StringCoordinateToBoolean = Record<StringCoordinate, boolean>

export type DownloadState = {
  queued: StringCoordinate[]
  pendingDownloads: StringCoordinateToBoolean
  emptyValues: StringCoordinateToBoolean
  knownValues: Record<StringCoordinate, SceneContentServerEntity>
}

export const DOWNLOAD_INITIAL_STATE: DownloadState = {
  queued: [],
  pendingDownloads: {},
  emptyValues: {},
  knownValues: {}
}

export function enqueueValues(state: DownloadState, coordinates: StringCoordinate[]) {
  const queued = unique(setFilter(coordinates, state.emptyValues, state.knownValues, state.pendingDownloads))
  return {
    ...state,
    queued
  }
}

export function markAsPending(state: DownloadState, coordinates: StringCoordinate[]) {
  return {
    ...state,
    queued: setRemove(state.queued, coordinates),
    pendingDownloads: mapAdd(state.pendingDownloads, coordinates)
  }
}

export function markAsEmpty(state: DownloadState, coordinates: StringCoordinate[]) {
  return {
    ...state,
    pendingDownloads: mapRemove(state.pendingDownloads, coordinates),
    emptyValues: mapAdd(state.emptyValues, coordinates)
  }
}

export function markAsKnown(state: DownloadState, entity: SceneContentServerEntity) {
  return {
    ...state,
    pendingDownloads: mapRemove(state.pendingDownloads, entity.pointers),
    knownValues: entity.pointers.reduce(
      (cumm, item) => {
        cumm[item] = entity
        return cumm
      },
      { ...state.knownValues }
    )
  }
}
