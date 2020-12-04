const qs: any = require('query-string')
import { worldToGrid, gridToWorld, parseParcelPosition } from 'atomicHelpers/parcelScenePositions'
import {
  Vector3,
  ReadOnlyVector3,
  ReadOnlyQuaternion,
  Vector2,
  ReadOnlyVector2
} from 'decentraland-ecs/src/decentraland/math'
import { Observable } from 'decentraland-ecs/src/ecs/Observable'
import { fetchSceneIds } from 'decentraland-loader/lifecycle/utils/fetchSceneIds'
import { fetchSceneJson } from 'decentraland-loader/lifecycle/utils/fetchSceneJson'
import { ILand } from 'shared/types'
import { InstancedSpawnPoint } from '../types'

declare var location: any
declare var history: any

export type PositionReport = {
  /** Camera position, world space */
  position: ReadOnlyVector3
  /** Camera rotation */
  quaternion: ReadOnlyQuaternion
  /** Camera rotation, euler from quaternion */
  rotation: ReadOnlyVector3
  /** Camera height, relative to the feet of the avatar or ground */
  playerHeight: number
  /** Should this position be applied immediately */
  immediate: boolean
}
export type ParcelReport = {
  /** Parcel where the user is */
  parcel: ReadOnlyVector2
  /** Should this position be applied immediately */
  immediate: boolean
}

// Agregar un scene observable???
export const positionObservable = new Observable<Readonly<PositionReport>>()
// Called each time the user changes  parcel
export const parcelObservable = new Observable<ParcelReport>()
// Called each time the user changes scene
export const sceneObservable = new Observable<ILand>()

export const teleportObservable = new Observable<ReadOnlyVector2>()

export const lastPlayerPosition = new Vector3()
export const lastPlayerParcel = new Vector2()
export let lastPlayerScene: ILand

positionObservable.add((event) => {
  lastPlayerPosition.copyFrom(event.position)
})

// Listen to position changes, and notify if the parcel changed
positionObservable.add(({ position, immediate }) => {
  const parcel = Vector2.Zero()
  worldToGrid(position, parcel)
  if (parcel.x !== lastPlayerParcel.x || parcel.y !== lastPlayerParcel.y) {
    lastPlayerParcel.copyFrom(parcel)
    parcelObservable.notifyObservers({ parcel, immediate })
  }
})

// Listen to parcel changes, and notify if the scene changed
parcelObservable.add(async ({ parcel }) => {
  const parcelString = `${parcel.x},${parcel.x}`
  if (!lastPlayerScene || !lastPlayerScene.sceneJsonData.scene.parcels.includes(parcelString)) {
    const scenesId = await fetchSceneIds([parcelString])
    const sceneId = scenesId[0]
    if (sceneId) {
      const land = (await fetchSceneJson([sceneId]))[0]
      lastPlayerScene = land
      sceneObservable.notifyObservers(land)
    }
  }
})

export function initializeUrlPositionObserver() {
  let lastTime: number = performance.now()

  function updateUrlPosition(newParcel: ReadOnlyVector2) {
    // Update position in URI every second
    if (performance.now() - lastTime > 1000) {
      const currentPosition = `${newParcel.x | 0},${newParcel.y | 0}`
      const stateObj = { position: currentPosition }

      const q = qs.parse(location.search)
      q.position = currentPosition

      history.replaceState(stateObj, 'position', `?${qs.stringify(q)}`)

      lastTime = performance.now()
    }
  }

  parcelObservable.add(({ parcel }) => {
    updateUrlPosition(parcel)
  })

  if (lastPlayerPosition.equalsToFloats(0, 0, 0)) {
    // LOAD INITIAL POSITION IF SET TO ZERO
    const query = qs.parse(location.search)

    if (query.position) {
      const parcelCoords = query.position.split(',')
      gridToWorld(parseFloat(parcelCoords[0]), parseFloat(parcelCoords[1]), lastPlayerPosition)
    } else {
      lastPlayerPosition.x = Math.round(Math.random() * 10) - 5
      lastPlayerPosition.z = 0
    }
  }
}

/**
 * Computes the spawn point based on a scene.
 *
 * The computation takes the spawning points defined in the scene document and computes the spawning point in the world based on the base parcel position.
 *
 * @param land Scene on which the player is spawning
 */
export function pickWorldSpawnpoint(land: ILand): InstancedSpawnPoint {
  const pick = pickSpawnpoint(land)

  const spawnpoint = pick || { position: { x: 0, y: 0, z: 0 } }

  const baseParcel = land.sceneJsonData.scene.base
  const [bx, by] = baseParcel.split(',')

  const basePosition = new Vector3()

  const { position, cameraTarget } = spawnpoint

  gridToWorld(parseInt(bx, 10), parseInt(by, 10), basePosition)

  return {
    position: basePosition.add(position),
    cameraTarget: cameraTarget ? basePosition.add(cameraTarget) : undefined
  }
}

function pickSpawnpoint(land: ILand): InstancedSpawnPoint | undefined {
  if (!land.sceneJsonData || !land.sceneJsonData.spawnPoints || land.sceneJsonData.spawnPoints.length === 0) {
    return undefined
  }

  // 1 - default spawn points
  const defaults = land.sceneJsonData.spawnPoints.filter(($) => $.default)

  // 2 - if no default spawn points => all existing spawn points
  const eligiblePoints = defaults.length === 0 ? land.sceneJsonData.spawnPoints : defaults

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

export function getLandBase(land: ILand): { x: number; y: number } {
  if (
    land.sceneJsonData &&
    land.sceneJsonData.scene &&
    land.sceneJsonData.scene.base &&
    typeof (land.sceneJsonData.scene.base as string | void) === 'string'
  ) {
    return parseParcelPosition(land.sceneJsonData.scene.base)
  } else {
    return parseParcelPosition(land.mappingsResponse.parcel_id)
  }
}
