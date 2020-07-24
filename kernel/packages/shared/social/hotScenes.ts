import { refreshCandidatesStatuses } from 'shared/dao'
import { ParcelArray } from 'shared/comms/interface/utils'
import { Candidate } from 'shared/dao/types'
import { StoreContainer } from 'shared/store/rootTypes'
import { fetchSceneIds } from 'decentraland-loader/lifecycle/utils/fetchSceneIds'
import { fetchSceneJson } from 'decentraland-loader/lifecycle/utils/fetchSceneJson'
import { SceneJsonData } from 'shared/types'
import { getSceneNameFromAtlasState, postProcessSceneName } from 'shared/atlas/selectors'

declare const globalThis: StoreContainer

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
  coordinates: {
    x: number
    y: number
  }
  realmsInfo: CrowdedSceneRealmInfo[]
}

type CandidateCrowdedScene = {
  id: string
  scene: SceneJsonData | undefined
  baseCoord: ParcelArray
  usersCount: number
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
    baseCoord: baseCoord.split(',').map((str) => parseInt(str, 10)) as ParcelArray,
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
  const scenesJson = await fetchSceneJson(scenesId.filter((id) => id !== null) as string[])

  let scenesJsonIdx = 0
  for (let i = 0; i < tiles.length; i++) {
    const id = scenesId[i] ?? tiles[i]
    const land = scenesId[i] ? scenesJson[scenesJsonIdx] : null

    if (land) {
      scenesJsonIdx++
    }

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
    globalThis.globalStore.getState().atlas.tileToScene[`${candidateScene.baseCoord[0]},${candidateScene.baseCoord[1]}`]
      .name

  return {
    name: postProcessSceneName(sceneName),
    coordinates: { x: candidateScene.baseCoord[0], y: candidateScene.baseCoord[1] },
    realmsInfo: []
  }
}
