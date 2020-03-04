import { ValidSceneState } from '../sceneInfo/types'
import { RootState } from '../state'

export function filterNotResolvedParcels(state: RootState, parcels: string[]) {
  const result: string[] = []
  for (let parcel of parcels) {
    if (!(state.sceneInfo as ValidSceneState).positionToSceneId[parcel]) {
      result.push(parcel)
    }
  }
  return result
}
