import { splitCoordinate } from '../utils/splitCoordinate'
import { gridToWorld } from '../utils/worldToGrid'

const PARCEL_SIZE = 16

type Vector3Component = { x: number; y: number; z: number }
function addVectors(v1: Vector3Component, v2: Vector3Component) {
  return { x: v1.x + v2.x, y: v1.y + v2.y, z: v1.z + v2.z }
}
type InstancedSpawnPoint = { position: Vector3Component; cameraTarget?: Vector3Component }

/**
 * Computes the spawn point based on a scene.
 *
 * The computation takes the spawning points defined in the scene document and computes the spawning point in the world based on the base parcel position.
 *
 * @param land Scene on which the player is spawning
 */
export function pickWorldSpawnpoint(land: {
  scene: { base?: string; parcels: string[] }
  spawnPoints?: {
    position: { x: number | number[]; y: number | number[]; z: number | number[] }
    cameraTarget?: { x: number; y: number; z: number }
    default?: boolean
  }[]
}): InstancedSpawnPoint {
  const pick = pickSpawnpoint(land)

  const spawnpoint = pick || { position: { x: PARCEL_SIZE / 2, y: 0, z: PARCEL_SIZE / 2 } }

  const baseParcel = land.scene.base || getBaseParcel(land.scene.parcels)
  const [bx, by] = baseParcel.split(',')

  const { position, cameraTarget } = spawnpoint

  const base = gridToWorld(parseInt(bx, 10), parseInt(by, 10))

  return {
    position: addVectors(base, position),
    cameraTarget: cameraTarget ? addVectors(base, position) : undefined
  }
}

function pickSpawnpoint(land: {
  spawnPoints?: {
    position: { x: number | number[]; y: number | number[]; z: number | number[] }
    cameraTarget?: { x: number; y: number; z: number }
    default?: boolean
  }[]
}): InstancedSpawnPoint | undefined {
  if (!land || !land.spawnPoints || land.spawnPoints.length === 0) {
    return undefined
  }

  // 1 - default spawn points
  const defaults = land.spawnPoints.filter($ => $.default)

  // 2 - if no default spawn points => all existing spawn points
  const eligiblePoints = defaults.length === 0 ? land.spawnPoints : defaults

  // 3 - pick randomly between spawn points
  const { position, cameraTarget } = eligiblePoints[Math.floor(Math.random() * eligiblePoints.length)]

  // 4 - generate random x, y, z components when in arrays
  return {
    position: {
      x: computeComponentValue(position.x),
      y: computeComponentValue(position.y),
      z: computeComponentValue(position.z)
    },
    cameraTarget
  }
}

function computeComponentValue(x: number | number[]) {
  if (typeof x === 'number') {
    return x
  }
  if (x.length !== 2) {
    throw new Error(`array must have two values ${JSON.stringify(x)}`)
  }
  const [min, max] = x
  if (max <= min) {
    throw new Error(`max value (${max}) must be greater than min value (${min})`)
  }
  return Math.random() * (max - min) + min
}

function getBaseParcel(parcels: string[]) {
  const sorted = parcels.map(splitCoordinate).sort((a, b) => (a[0] === b[0] ? a[0] - b[0] : a[1] - b[1]))
  return sorted[0].join(',')
}
