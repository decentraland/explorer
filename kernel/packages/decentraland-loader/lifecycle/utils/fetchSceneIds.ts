import { getServer } from '../manager'

export async function fetchSceneIds(tiles: string[]): Promise<Array<string | null>> {
  const server = getServer()
  if (!server) return []
  const promises = server.getSceneIds(tiles)
  return Promise.all(promises)
}
