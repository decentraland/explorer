import { getServer } from '../manager'

export async function invalidateScene(sceneId: string) {
  const server = getServer()
  if (!server) return
  return server.invalidateScene(sceneId)
}
