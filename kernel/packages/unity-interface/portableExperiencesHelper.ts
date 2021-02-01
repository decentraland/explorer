import {
  ContentMapping,
  EnvironmentData,
  LoadablePortableExperienceScene,
  MappingsResponse,
  SceneJsonData
} from '../shared/types'
import { getSceneNameFromJsonData } from '../shared/selectors'
import { parseParcelPosition } from '../atomicHelpers/parcelScenePositions'
import { OffChainAsset } from '@dcl/urn-resolver'
import defaultLogger from 'shared/logger'

const STATIC_PORTABLE_SCENES_S3_BUCKET_URL = 'https://static-pe.decentraland.io'
export type PortableExperienceUrn = string

export async function getPortableExperienceFromS3Bucket(pe: OffChainAsset) {
  defaultLogger.info('GETTING PE...', pe, pe.id)
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
