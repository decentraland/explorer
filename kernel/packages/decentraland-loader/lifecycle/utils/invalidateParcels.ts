import { LifecycleManager, getServer } from '../manager'

export function invalidateParcels(tiles: string[]) {
  const server: LifecycleManager = getServer()
  server.invalidateParcels(tiles)
}
