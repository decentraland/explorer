type Vector2Component = {
  x: number
  y: number
}

const LINE_OF_SIGHT_RADIUS = 4

export function squareAndSum(a: number, b: number) {
  return a * a + b * b
}

const cachedDeltas: { [limit: number]: Vector2Component[] } = {}

export function parcelsInSight(position: Vector2Component): string[] {
  const result: string[] = []
  if (!cachedDeltas[LINE_OF_SIGHT_RADIUS]) {
    cachedDeltas[LINE_OF_SIGHT_RADIUS] = []
  }
  let length = cachedDeltas[LINE_OF_SIGHT_RADIUS].length
  if (!length) {
    calculateCachedDeltas(LINE_OF_SIGHT_RADIUS)
    length = cachedDeltas[LINE_OF_SIGHT_RADIUS].length
  }
  for (let i = 0; i < length; i++) {
    result.push(
      `${position.x + cachedDeltas[LINE_OF_SIGHT_RADIUS][i].x},${position.y + cachedDeltas[LINE_OF_SIGHT_RADIUS][i].y}`
    )
  }
  return result
}

function calculateCachedDeltas(limit: number) {
  const squaredRadius = limit * limit
  for (let x = -limit; x <= limit; x++) {
    for (let y = -limit; y <= limit; y++) {
      if (x * x + y * y <= squaredRadius) {
        cachedDeltas[limit].push({ x, y })
      }
    }
  }
  cachedDeltas[limit].sort((a, b) => squareAndSum(a.x, a.y) - squareAndSum(b.x, b.y))
}
