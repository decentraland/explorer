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
import { parseUrn, DecentralandAssetIdentifier, OffChainAsset } from '@dcl/urn-resolver'

const STATIC_PORTABLE_SCENES_S3_BUCKET_URL = 'https://static-pe.decentraland.io'
export type PortableExperienceUrn = string

export async function spawnPortableExperienceScene(portableExperienceUrn: PortableExperienceUrn): Promise<string> {
  const parsedUrn: DecentralandAssetIdentifier | null = await parseUrn(portableExperienceUrn)

  if (!parsedUrn || !isPortableExperience(parsedUrn)) {
    throw new Error(`Could not parse portable experience from urn: ${portableExperienceUrn}`)
  }

  const scene = new UnityPortableExperienceScene(
    await getPortableExperienceFromS3Bucket((parsedUrn as unknown) as OffChainAsset)
  )
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
  return parcelSceneId
}

function isPortableExperience(dclId: DecentralandAssetIdentifier): dclId is OffChainAsset {
  if (!!dclId) {
    const offChainAsset = dclId as OffChainAsset
    return !!offChainAsset.registry && offChainAsset.registry === 'static-portable-experiences'
  }
  return false
}

export async function killPortableExperienceScene(peId: string): Promise<void> {
  const peWorker = getSceneWorkerBySceneID(peId)

  if (peWorker) {
    forceStopParcelSceneWorker(peWorker)
  }
}

export async function getPortableExperienceFromS3Bucket(pe: OffChainAsset) {
  const peId: string = pe.id
  const baseUrl: string = `${STATIC_PORTABLE_SCENES_S3_BUCKET_URL}/${peId}/`

  const mappingsFetch = await fetch(`${baseUrl}mappings`)
  const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

  const sceneJsonMapping = mappingsResponse.contents.find(($) => $.file === 'scene.json')

  if (sceneJsonMapping) {
    const sceneResponse = await fetch(`${baseUrl}${sceneJsonMapping.hash}`)

    if (sceneResponse.ok) {
      const scene = (await sceneResponse.json()) as SceneJsonData
      return getLoadablePortableExperience({
        peId,
        baseUrl: `${baseUrl}`,
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

export function getLoadablePortableExperience(data: {
  peId: string
  mappings: ContentMapping[]
  sceneJsonData: SceneJsonData
  baseUrl: string
}): EnvironmentData<LoadablePortableExperienceScene> {
  const { peId, mappings, sceneJsonData, baseUrl } = data

  const sceneJsons = mappings.filter((land) => land.file === 'scene.json')
  if (!sceneJsons.length) {
    throw new Error('Invalid scene mapping: no scene.json')
  }

  const cid = peId // TODO: Load this from content server

  return {
    sceneId: cid,
    baseUrl: baseUrl,
    name: getSceneNameFromJsonData(sceneJsonData),
    main: sceneJsonData.main,
    useFPSThrottling: false,
    mappings,
    data: {
      id: cid,
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
