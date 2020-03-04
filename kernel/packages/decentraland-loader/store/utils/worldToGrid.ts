const PARCEL_SIZE = 16

/**
 * Transforms a grid position into a world-relative 3d position
 */
export function gridToWorld(x: number, y: number) {
  return {
    x: PARCEL_SIZE * x,
    y: 0,
    z: PARCEL_SIZE * y
  }
}

/**
 * Transforms a world position into a grid position
 */
export function worldToGrid(vector: { x: number; z: number }) {
  return {
    x: Math.floor(vector.x / PARCEL_SIZE),
    y: Math.floor(vector.z / PARCEL_SIZE)
  }
}

export function worldToStringCoordinate(vector: { x: number; y: number; z: number }) {
  const { x, y } = worldToGrid(vector)
  return `${x},${y}`
}

export function xyToStringCoordinate(vector: { x: number; y: number }) {
  return `${vector.x},${vector.y}`
}

export function stringCoordinateToXY(position: string) {
  const [x, y] = position.split(',').map(_ => parseInt(_, 10))
  if (isNaN(x) || isNaN(y)) {
    throw new Error('invalid!')
  }
  return { x, y }
}
