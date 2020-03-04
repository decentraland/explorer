import { StringCoordinate } from '../sceneInfo/types'
export function splitCoordinate(coordinate: StringCoordinate): [number, number] {
  if (!coordinate.match(/^-?\d+,-?\d+$/g)) {
    throw new Error('Invalid coordinate provided: ' + coordinate)
  }
  return coordinate.split(',').map(_ => parseInt(_, 10)) as [number, number]
}
