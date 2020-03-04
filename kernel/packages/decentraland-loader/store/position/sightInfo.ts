import { StringCoordinate } from '../sceneInfo/types'
import { arrayToDictionary } from '../utils/arrayToDictionary'
import { parcelsInSight } from '../utils/parcelsInSight'
import { setDifference } from '../utils/setDifference'
import { splitCoordinate } from '../utils/splitCoordinate'

export type SightInfo = {
  inSight: StringCoordinate[]
  inSightDict: Record<StringCoordinate, boolean>
  recentlySighted: StringCoordinate[]
  recentlyLostSight: StringCoordinate[]
}

export function initialSight(coordinate: StringCoordinate): SightInfo {
  const [x, y] = splitCoordinate(coordinate)
  const inSight = parcelsInSight({ x, y })
  return {
    inSight,
    inSightDict: arrayToDictionary(inSight),
    recentlySighted: inSight,
    recentlyLostSight: []
  }
}

export function sightChange(sight: SightInfo, coordinate: StringCoordinate): SightInfo {
  const [x, y] = splitCoordinate(coordinate)
  const inSight = parcelsInSight({ x, y })
  const inSightDict = arrayToDictionary(inSight)
  const recentlyLostSight = [
    ...sight.recentlyLostSight,
    ...setDifference(sight.inSightDict, inSightDict, sight.inSight)
  ]
  const recentlySighted = [...sight.recentlySighted, ...setDifference(inSightDict, sight.inSightDict, inSight)]
  return {
    inSight,
    inSightDict,
    recentlySighted,
    recentlyLostSight
  }
}

export function clearRecent(sight: SightInfo) {
  return {
    ...sight,
    recentlySighted: [],
    recentlyLostSight: []
  }
}
