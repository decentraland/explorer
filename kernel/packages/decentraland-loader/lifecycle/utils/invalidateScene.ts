import { LifecycleManager, getServer } from '../manager'

export function invalidateScene(sceneId: string) {
  const server: LifecycleManager = getServer()
  return server.invalidateScene(sceneId)
}
