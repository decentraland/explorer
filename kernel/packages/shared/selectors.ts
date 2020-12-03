import { parseParcelPosition } from 'atomicHelpers/parcelScenePositions'
import { ContentMapping, ILand, EnvironmentData, LoadableParcelScene, SceneJsonData } from './types'

export function normalizeContentMappings(
  mappings: Record<string, string> | Array<ContentMapping>
): Array<ContentMapping> {
  const ret: Array<ContentMapping> = []

  if (typeof mappings.length === 'number' || mappings instanceof Array) {
    ret.push(...(mappings as any))
  } else {
    for (let key in mappings) {
      const file = key.toLowerCase()

      ret.push({ file, hash: mappings[key] })
    }
  }

  return ret
}

export function ILandToLoadableParcelScene(land: ILand): EnvironmentData<LoadableParcelScene> {
  const mappings: ContentMapping[] = normalizeContentMappings(land.mappingsResponse.contents)
  const sceneJsons = land.mappingsResponse.contents.filter((land) => land.file === 'scene.json')
  if (!sceneJsons.length) {
    throw new Error('Invalid scene mapping: no scene.json')
  }

  const ret: EnvironmentData<LoadableParcelScene> = {
    sceneId: land.sceneId,
    baseUrl: land.baseUrl,
    name: getSceneNameFromJsonData(land.sceneJsonData),
    main: land.sceneJsonData.main,
    useFPSThrottling: false,
    mappings,
    data: {
      id: land.sceneId,
      basePosition: parseParcelPosition(land.sceneJsonData.scene.base),
      name: getSceneNameFromJsonData(land.sceneJsonData),
      parcels:
        (land.sceneJsonData &&
          land.sceneJsonData.scene &&
          land.sceneJsonData.scene.parcels &&
          land.sceneJsonData.scene.parcels.map(parseParcelPosition)) ||
        [],
      baseUrl: land.baseUrl,
      baseUrlBundles: land.baseUrlBundles,
      contents: mappings,
      land
    }
  }

  return ret
}

export function ILandToLoadableParcelSceneUpdate(land: ILand): EnvironmentData<LoadableParcelScene> {
  const mappings: ContentMapping[] = normalizeContentMappings(land.mappingsResponse.contents)

  const ret: EnvironmentData<LoadableParcelScene> = {
    sceneId: land.sceneId,
    baseUrl: land.baseUrl,
    name: getSceneNameFromJsonData(land.sceneJsonData),
    main: land.sceneJsonData.main,
    useFPSThrottling: false,
    mappings,
    data: {
      id: land.sceneId,
      basePosition: parseParcelPosition('0,0'),
      name: getSceneNameFromJsonData(land.sceneJsonData),
      parcels:
        (land.sceneJsonData &&
          land.sceneJsonData.scene &&
          land.sceneJsonData.scene.parcels &&
          land.sceneJsonData.scene.parcels.map(parseParcelPosition)) ||
        [],
      baseUrl: land.baseUrl,
      baseUrlBundles: land.baseUrlBundles,
      contents: mappings,
      land
    }
  }

  return ret
}

export function getOwnerNameFromJsonData(jsonData?: SceneJsonData) {
  let ownerName = jsonData?.contact?.name
  if (ownerName === 'author-name') {
    // avoid using autogenerated name
    ownerName = undefined
  }

  return ownerName || 'Unknown'
}

export function getSceneDescriptionFromJsonData(jsonData?: SceneJsonData) {
  return jsonData?.display?.description || ''
}

export function getSceneNameFromJsonData(jsonData?: SceneJsonData) {
  let title = jsonData?.display?.title
  if (title === 'interactive-text') {
    // avoid using autogenerated name
    title = undefined
  }

  return title || 'Unnamed'
}

export function getThumbnailUrlFromJsonDataAndContent(
  jsonData: SceneJsonData | undefined,
  contents: Array<ContentMapping> | undefined,
  downloadUrl: string
): string | undefined {
  if (!jsonData) {
    return undefined
  }

  if (!contents || !downloadUrl) {
    return getThumbnailUrlFromJsonData(jsonData)
  }

  let thumbnail: string | undefined = jsonData.display?.navmapThumbnail
  if (thumbnail && !thumbnail.startsWith('http')) {
    // We are assuming that the thumbnail is an uploaded file. We will try to find the matching hash
    const thumbnailHash = contents?.find(({ file }) => file === thumbnail)?.hash
    if (thumbnailHash) {
      thumbnail = `${downloadUrl}/contents/${thumbnailHash}`
    } else {
      // If we couldn't find a file with the correct path, then we ignore whatever was set on the thumbnail property
      thumbnail = undefined
    }
  }

  if (!thumbnail) {
    thumbnail = getThumbnailUrlFromBuilderProjectId(jsonData.source?.projectId)
  }
  return thumbnail
}

export function getThumbnailUrlFromJsonData(jsonData?: SceneJsonData): string | undefined {
  if (!jsonData) {
    return undefined
  }

  return jsonData.display?.navmapThumbnail ?? getThumbnailUrlFromBuilderProjectId(jsonData.source?.projectId)
}

export function getThumbnailUrlFromBuilderProjectId(projectId: string | undefined): string | undefined {
  if (!projectId) {
    return undefined
  }

  return `https://builder-api.decentraland.org/v1/projects/${projectId}/media/preview.png`
}
