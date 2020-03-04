import { RootState } from '../state'

export function filterLostSightedParcels(state: RootState) {
  return state.sightInfo.recentlyLostSight
}
