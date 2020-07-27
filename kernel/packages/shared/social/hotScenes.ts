import { refreshCandidatesStatuses } from 'shared/dao'
import { Candidate } from 'shared/dao/types'
import { StoreContainer } from 'shared/store/rootTypes'
import { fetchSceneIds } from 'decentraland-loader/lifecycle/utils/fetchSceneIds'
import { fetchSceneJson } from 'decentraland-loader/lifecycle/utils/fetchSceneJson'
import { SceneJsonData } from 'shared/types'
import { reportScenesFromTiles } from 'shared/atlas/actions'
import { getSceneNameFromAtlasState, postProcessSceneName } from 'shared/atlas/selectors'

declare const globalThis: StoreContainer

declare const window: {
  unityInterface: {
    UpdateHotScenesList: (info: HotSceneInfo[]) => void
  }
}

type CrowdedSceneRealmInfo = {
  realm: {
    serverName: string
    layerName: string
  }
  usersCount: number
  maxUsers: number
}

type CrowdedSceneInfo = {
  name: string
  baseCoord: string
  realmsInfo: CrowdedSceneRealmInfo[]
}

type CandidateCrowdedScene = {
  id: string
  scene: SceneJsonData | undefined
  baseCoord: string
  usersCount: number
}

export type HotSceneInfo = {
  baseCoords: { x: number; y: number }
  usersTotalCount: number
  realms: { serverName: string; layer: string; usersCount: number; usersMax: number }[]
}

export async function fetchHotScenes(): Promise<CrowdedSceneInfo[]> {
  const candidates = await refreshCandidatesStatuses()

  let crowdedScenes: Record<string, CrowdedSceneInfo> = {}

  const filteredCandidates = candidates.filter(
    (candidate) => candidate.layer && candidate.layer.usersCount > 0 && candidate.layer.usersParcels
  )

  for (const candidate of filteredCandidates) {
    const candidateScenes = await getCrowdedScenesFromCandidate(candidate)

    candidateScenes.forEach((sceneInfo) => {
      if (!crowdedScenes[sceneInfo.id]) {
        crowdedScenes[sceneInfo.id] = crowdedSceneInfoFromCandidateScene(sceneInfo)
      }
      crowdedScenes[sceneInfo.id].realmsInfo.push({
        realm: {
          serverName: candidate.catalystName,
          layerName: candidate.layer.name
        },
        maxUsers: candidate.layer.maxUsers,
        usersCount: sceneInfo.usersCount
      })
    })
  }

  const sceneValues = Object.values(crowdedScenes)
  sceneValues.forEach((scene) => scene.realmsInfo.sort((a, b) => (a.usersCount > b.usersCount ? -1 : 1)))

  return sceneValues.sort((a, b) => (countUsers(a) > countUsers(b) ? -1 : 1))
}

export async function reportHotScenes() {
  const hotScenes = await fetchHotScenes()

  globalThis.globalStore.dispatch(reportScenesFromTiles(hotScenes.map((scene) => scene.baseCoord)))
  window.unityInterface.UpdateHotScenesList(hotScenes.map((scene) => hotSceneInfoFromCrowdedSceneInfo(scene)))
}

function countUsers(a: CrowdedSceneInfo) {
  return a.realmsInfo.reduce((total, realmInfo) => total + realmInfo.usersCount, 0)
}

function createCandidateCrowdedScene(
  id: string,
  baseCoord: string,
  sceneJsonData: SceneJsonData | undefined
): CandidateCrowdedScene {
  return {
    id,
    baseCoord: baseCoord,
    scene: sceneJsonData,
    usersCount: 1
  }
}

async function getCrowdedScenesFromCandidate(candidate: Candidate): Promise<CandidateCrowdedScene[]> {
  let scenes: Record<string, CandidateCrowdedScene> = {}

  const tiles =
    candidate.layer.usersParcels?.filter((value) => value[0] && value[1]).map((value) => `${value[0]},${value[1]}`) ??
    []

  const scenesId = await fetchSceneIds(tiles)

  for (let i = 0; i < tiles.length; i++) {
    const id = scenesId[i] ?? tiles[i]
    const land = scenesId[i] ? (await fetchSceneJson([scenesId[i]!]))[0] : null

    if (scenes[id]) {
      scenes[id].usersCount += 1
    } else {
      scenes[id] = createCandidateCrowdedScene(id, land?.sceneJsonData?.scene.base ?? tiles[i], land?.sceneJsonData)
    }
  }

  return Object.values(scenes)
}

function crowdedSceneInfoFromCandidateScene(candidateScene: CandidateCrowdedScene): CrowdedSceneInfo {
  const sceneName =
    getSceneNameFromAtlasState(candidateScene.scene) ??
    globalThis.globalStore.getState().atlas.tileToScene[candidateScene.baseCoord].name

  return {
    name: postProcessSceneName(sceneName),
    baseCoord: candidateScene.baseCoord,
    realmsInfo: []
  }
}

function hotSceneInfoFromCrowdedSceneInfo(crowdedSceneInfo: CrowdedSceneInfo): HotSceneInfo {
  const baseCoord = crowdedSceneInfo.baseCoord.split(',').map((str) => parseInt(str, 10)) as [number, number]
  return {
    baseCoords: { x: baseCoord[0], y: baseCoord[1] },
    usersTotalCount: countUsers(crowdedSceneInfo),
    realms: crowdedSceneInfo.realmsInfo.map((realmInfo) => {
      return {
        serverName: realmInfo.realm.serverName,
        layer: realmInfo.realm.layerName,
        usersCount: realmInfo.usersCount,
        usersMax: realmInfo.maxUsers
      }
    })
  }
}
