import { getServer } from '../manager'

export async function fetchSceneIds(parcels: string[]): Promise<Array<string | null>> {
  const server = getServer()
  if (!server) return []
  const promises = server.getSceneIds(parcels)
  return Promise.all(promises)
}
