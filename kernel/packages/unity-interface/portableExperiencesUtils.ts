import {
  ContentMapping,
  EnvironmentData,
  LoadablePortableExperienceScene,
  MappingsResponse,
  SceneJsonData
} from '../shared/types'
import { getSceneNameFromJsonData } from '../shared/selectors'
import { parseParcelPosition } from '../atomicHelpers/parcelScenePositions'
import { UnityPortableExperienceScene } from './UnityParcelScene'
import {
  forceStopParcelSceneWorker,
  getParcelSceneID,
  getSceneWorkerBySceneID,
  loadParcelScene
} from 'shared/world/parcelSceneManager'
import { unityInterface } from './UnityInterface'
import { DecentralandAssetIdentifier, resolveContentUrl, parseUrn, OffChainAsset } from '@dcl/urn-resolver'

declare var window: any
window['spawnPortableExperienceScene'] = spawnPortableExperienceScene
window['killPortableExperienceScene'] = killPortableExperienceScene

export type PortableExperienceHandle = {
  pid: string
  parentCid: string
}

let currentPortableExperiences: Map<string, string> = new Map()

export async function spawnPortableExperienceScene(
  sceneUrn: string,
  parentCid: string
): Promise<PortableExperienceHandle> {
  const scene = new UnityPortableExperienceScene(await getPortableExperienceFromS3Bucket(sceneUrn))

  loadParcelScene(scene, undefined, true)
  const parcelSceneId = getParcelSceneID(scene)
  unityInterface.CreateUIScene({
    id: parcelSceneId,
    name: scene.data.name,
    baseUrl: scene.data.baseUrl,
    contents: scene.data.data.contents,
    icon: scene.data.data.icon,
    isPortableExperience: true
  })
  currentPortableExperiences.set(sceneUrn, parentCid)

  return { pid: sceneUrn, parentCid: parentCid }
}

export async function killPortableExperienceScene(sceneUrn: string): Promise<boolean> {
  const peWorker = getSceneWorkerBySceneID(sceneUrn)
  if (peWorker) {
    forceStopParcelSceneWorker(peWorker)
    currentPortableExperiences.delete(sceneUrn)
    unityInterface.UnloadScene(sceneUrn)
    return true
  } else {
    return false
  }
}

export async function getPortableExperience(pid: string): Promise<PortableExperienceHandle | undefined> {
  if (currentPortableExperiences.has(pid)) {
    return { pid: pid, parentCid: currentPortableExperiences.get(pid)! }
  } else {
    return undefined
  }
}

async function parsePortableExperienceUrn(sceneUrn: string): Promise<DecentralandAssetIdentifier> {
  const parsedUrn: DecentralandAssetIdentifier | null = await parseUrn(sceneUrn)
  if (!parsedUrn || !isPortableExperience(parsedUrn)) {
    throw new Error(`Could not parse portable experience from urn: ${sceneUrn}`)
  }
  return parsedUrn
}

function isPortableExperience(dclId: DecentralandAssetIdentifier): dclId is OffChainAsset {
  /* tslint:disable-next-line */
  return !!(dclId as OffChainAsset).registry && (dclId as OffChainAsset).registry === 'static-portable-experiences'
}

export async function getPortableExperienceFromS3Bucket(sceneUrn: string) {
  const parsedUrn: DecentralandAssetIdentifier = await parsePortableExperienceUrn(sceneUrn)
  const mappingsUrl = await resolveContentUrl(parsedUrn)
  if (mappingsUrl === null) {
    throw new Error(`Could not resolve mappings for scene: ${sceneUrn}`)
  }
  const baseUrl: string = new URL('..', mappingsUrl).toString()
  const mappingsFetch = await fetch(baseUrl)
  const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

  const sceneJsonMapping = mappingsResponse.contents.find(($) => $.file === 'scene.json')

  if (sceneJsonMapping) {
    const sceneResponse = await fetch(`${baseUrl}${sceneJsonMapping.hash}`)

    if (sceneResponse.ok) {
      const scene = (await sceneResponse.json()) as SceneJsonData
      return getLoadablePortableExperience({
        sceneUrn: sceneUrn,
        baseUrl,
        mappings: mappingsResponse.contents,
        sceneJsonData: scene
      })
    } else {
      throw new Error('Could not load scene.json')
    }
  } else {
    throw new Error('Could not load scene.json')
  }
}

export async function getLoadablePortableExperience(data: {
  sceneUrn: string
  baseUrl: string
  mappings: ContentMapping[]
  sceneJsonData: SceneJsonData
}): Promise<EnvironmentData<LoadablePortableExperienceScene>> {
  const { sceneUrn, baseUrl, mappings, sceneJsonData } = data

  const sceneJsons = mappings.filter((land) => land.file === 'scene.json')
  if (!sceneJsons.length) {
    throw new Error('Invalid scene mapping: no scene.json')
  }
  // TODO: obtain sceneId from Content Server
  return {
    sceneId: sceneUrn,
    baseUrl: baseUrl,
    name: getSceneNameFromJsonData(sceneJsonData),
    main: sceneJsonData.main,
    useFPSThrottling: false,
    mappings,
    data: {
      id: sceneUrn,
      basePosition: parseParcelPosition(sceneJsonData.scene.base),
      name: getSceneNameFromJsonData(sceneJsonData),
      parcels:
        (sceneJsonData &&
          sceneJsonData.scene &&
          sceneJsonData.scene.parcels &&
          sceneJsonData.scene.parcels.map(parseParcelPosition)) ||
        [],
      baseUrl: baseUrl,
      baseUrlBundles: '',
      contents: mappings,
      icon: sceneJsonData.display?.favicon
    }
  }
}
