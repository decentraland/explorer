import { StoreContainer } from 'shared/store/rootTypes'
import { fetchSceneIds } from 'decentraland-loader/lifecycle/utils/fetchSceneIds'
import { fetchSceneJson } from 'decentraland-loader/lifecycle/utils/fetchSceneJson'
import { SceneJsonData } from 'shared/types'
import { reportScenesFromTiles } from 'shared/atlas/actions'
import { getSceneNameFromAtlasState, postProcessSceneName, getPoiTiles } from 'shared/atlas/selectors'
import { getHotScenesService, getUpdateProfileServer } from 'shared/dao/selectors'
import {
  getOwnerNameFromJsonData,
  getThumbnailUrlFromJsonDataAndContent,
  getSceneDescriptionFromJsonData
} from 'shared/selectors'

declare const globalThis: StoreContainer

declare const window: {
  unityInterface: {
    UpdateHotScenesList: (info: HotSceneInfo[]) => void
  }
}

type RealmInfo = {
  serverName: string
  layer: string
  usersCount: number
  usersMax: number
  userParcels: { x: number; y: number }[]
}

export type HotSceneInfo = {
  id: string
  name: string
  creator: string
  description: string
  thumbnail: string
  baseCoords: { x: number; y: number }
  parcels: { x: number; y: number }[]
  usersTotalCount: number
  realms: RealmInfo[]
}

export async function fetchHotScenes(): Promise<HotSceneInfo[]> {
  const url = getHotScenesService(globalThis.globalStore.getState())
  const response = await fetch(url)
  if (response.ok) {
    const info = await response.json()
    return info.map((scene: any) => {
      return {
        ...scene,
        baseCoords: { x: scene.baseCoords[0], y: scene.baseCoords[1] },
        parcels: scene.parcels.map((parcel: [number, number]) => {
          return { x: parcel[0], y: parcel[1] }
        }),
        realms: scene.realms.map((realm: any) => {
          return {
            ...realm,
            userParcels: realm.userParcels.map((parcel: [number, number]) => {
              return { x: parcel[0], y: parcel[1] }
            })
          } as RealmInfo
        })
      } as HotSceneInfo
    })
  } else {
    throw new Error(`Error fetching hot scenes. Response not OK. Status: ${response.status}`)
  }
}

export async function reportHotScenes() {
  const hotScenes = await fetchHotScenes()

  // NOTE: we report POI as hotscenes for now, approach should change in next iteration
  const pois = await fetchPOIsAsHotSceneInfo()
  const report = hotScenes.concat(pois.filter((poi) => hotScenes.filter((scene) => scene.id === poi.id).length === 0))

  globalThis.globalStore.dispatch(
    reportScenesFromTiles(report.map((scene) => `${scene.baseCoords.x},${scene.baseCoords.y}`))
  )
  window.unityInterface.UpdateHotScenesList(report)
}

function getSceneName(baseCoord: string, sceneJsonData: SceneJsonData | undefined): string {
  const sceneName =
    getSceneNameFromAtlasState(sceneJsonData) ?? globalThis.globalStore.getState().atlas.tileToScene[baseCoord]?.name
  return postProcessSceneName(sceneName)
}

async function fetchPOIsAsHotSceneInfo(): Promise<HotSceneInfo[]> {
  const tiles = getPoiTiles(globalThis.globalStore.getState())
  const scenesId = (await fetchSceneIds(tiles)).filter((id) => id !== null) as string[]
  const scenesLand = (await fetchSceneJson(scenesId)).filter((land) => land.sceneJsonData)

  return scenesLand.map((land) => {
    return {
      id: land.sceneId,
      name: getSceneName(land.sceneJsonData.scene.base, land.sceneJsonData),
      creator: getOwnerNameFromJsonData(land.sceneJsonData),
      description: getSceneDescriptionFromJsonData(land.sceneJsonData),
      thumbnail:
        getThumbnailUrlFromJsonDataAndContent(
          land.sceneJsonData,
          land.mappingsResponse.contents,
          getUpdateProfileServer(globalThis.globalStore.getState())
        ) ?? '',
      baseCoords: TileStringToVector2(land.sceneJsonData.scene.base),
      parcels: land.sceneJsonData
        ? land.sceneJsonData.scene.parcels.map((parcel) => {
          const coord = parcel.split(',').map((str) => parseInt(str, 10)) as [number, number]
          return { x: coord[0], y: coord[1] }
        })
        : [],
      realms: [{ serverName: '', layer: '', usersMax: 0, usersCount: 0, userParcels: [] }],
      usersTotalCount: 0
    }
  })
}

function TileStringToVector2(tileValue: string): { x: number; y: number } {
  const tile = tileValue.split(',').map((str) => parseInt(str, 10)) as [number, number]
  return { x: tile[0], y: tile[1] }
}
