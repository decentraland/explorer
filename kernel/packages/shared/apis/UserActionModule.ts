import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import defaultLogger from 'shared/logger'
import { getOwnerNameFromJsonData, getThumbnailUrlFromJsonDataAndContent } from 'shared/selectors'
import { getUpdateProfileServer } from 'shared/dao/selectors'
import { fetchSceneIds } from 'decentraland-loader/lifecycle/utils/fetchSceneIds'
import { fetchSceneJson } from 'decentraland-loader/lifecycle/utils/fetchSceneJson'
import { getSceneNameFromAtlasState, postProcessSceneName } from 'shared/atlas/selectors'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { store } from 'shared/store/isolatedStore'

export interface IUserActionModule {
  requestTeleport(destination: string): Promise<void>
}

@registerAPI('UserActionModule')
export class UserActionModule extends ExposableAPI implements IUserActionModule {
  @exposeMethod
  async requestTeleport(destination: string): Promise<void> {
    if (destination === 'magic' || destination === 'crowd') {
      getUnityInstance().RequestTeleport({ destination })
      return
    } else if (!/^\-?\d+\,\-?\d+$/.test(destination)) {
      defaultLogger.error(`teleportTo: invalid destination ${destination}`)
      return
    }

    let sceneThumbnailUrl: string | undefined
    let sceneName: string = destination
    let sceneCreator: string = 'Unknown'
    let sceneEvent = {}

    const sceneId = (await fetchSceneIds([destination]))[0]
    const mapSceneData = sceneId ? (await fetchSceneJson([sceneId!]))[0] : undefined

    sceneName = this.getSceneName(destination, mapSceneData?.sceneJsonData)
    sceneCreator = getOwnerNameFromJsonData(mapSceneData?.sceneJsonData)

    if (mapSceneData) {
      sceneThumbnailUrl = getThumbnailUrlFromJsonDataAndContent(
        mapSceneData.sceneJsonData,
        mapSceneData.mappingsResponse.contents,
        getUpdateProfileServer(store.getState())
      )
    }
    if (!sceneThumbnailUrl) {
      let sceneParcels = [destination]
      if (mapSceneData && mapSceneData.sceneJsonData?.scene.parcels) {
        sceneParcels = mapSceneData.sceneJsonData.scene.parcels
      }
      sceneThumbnailUrl = `https://api.decentraland.org/v1/map.png?width=480&height=237&size=10&center=${destination}&selected=${sceneParcels.join(
        ';'
      )}`
    }

    try {
      const response = await fetch(`https://events.decentraland.org/api/events/?position=${destination}`)
      const json = await response.json()
      if (json.data.length > 0) {
        sceneEvent = {
          name: json.data[0].name,
          total_attendees: json.data[0].total_attendees,
          start_at: json.data[0].start_at,
          finish_at: json.data[0].finish_at
        }
      }
    } catch (e) {
      defaultLogger.error(e)
    }

    getUnityInstance().RequestTeleport({
      destination,
      sceneEvent,
      sceneData: {
        name: sceneName,
        owner: sceneCreator,
        previewImageUrl: sceneThumbnailUrl ?? ''
      }
    })
  }

  private getSceneName(baseCoord: string, sceneJsonData: any): string {
    const sceneName = getSceneNameFromAtlasState(sceneJsonData) ?? store.getState().atlas.tileToScene[baseCoord]?.name
    return postProcessSceneName(sceneName)
  }
}
