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
  const sceneJsons = land.mappingsResponse.contents.filter(land => land.file === 'scene.json')
  if (!sceneJsons.length) {
    throw new Error('Invalid scene mapping: no scene.json')
  }

  const ret: EnvironmentData<LoadableParcelScene> = {
    sceneId: land.sceneId,
    baseUrl: land.baseUrl,
    name: getSceneTitle(land.sceneJsonData),
    main: land.sceneJsonData.main,
    useFPSThrottling: false,
    mappings,
    data: {
      id: land.sceneId,
      basePosition: parseParcelPosition(land.sceneJsonData.scene.base),
      name: getSceneTitle(land.sceneJsonData),
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
    name: getSceneTitle(land.sceneJsonData),
    main: land.sceneJsonData.main,
    useFPSThrottling: false,
    mappings,
    data: {
      id: land.sceneId,
      basePosition: parseParcelPosition('0,0'),
      name: getSceneTitle(land.sceneJsonData),
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

export function getOwnerName(jsonData: SceneJsonData) {
  return jsonData.contact?.name || 'Unknown'
}

export function getSceneDescription(jsonData: SceneJsonData) {
  return jsonData.display?.description || ''
}

export function getSceneTitle(jsonData: SceneJsonData) {
  return jsonData.display?.title || 'Unnamed'
}
