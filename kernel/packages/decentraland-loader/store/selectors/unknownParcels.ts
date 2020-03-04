import { RootState } from '../state'

export function unknownParcels(state: RootState) {
  return state.sightInfo.recentlySighted.filter(
    _ => !state.download.knownValues[_] && !state.download.emptyValues[_] && !state.download.pendingDownloads[_]
  )
}
