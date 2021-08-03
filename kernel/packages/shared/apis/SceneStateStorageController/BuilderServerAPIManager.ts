import { Authenticator } from 'dcl-crypto'
import { ExplorerIdentity } from 'shared/session/types'
import { uuid } from 'decentraland-ecs/src/ecs/helpers'
import {
  BuilderAsset,
  BuilderManifest,
  BuilderProject,
  BuilderScene,
  AssetId,
  Asset,
  SerializedSceneState,
  BuilderMetric,
  BuilderEntity,
  BuilderComponent,
  BuilderGround
} from './types'
import { getDefaultTLD } from 'config'
import { defaultLogger } from '../../logger'
import { getParcelSceneLimits } from 'atomicHelpers/landHelpers'
import { CLASS_ID } from 'decentraland-ecs/src'
import { toHumanReadableType, fromHumanReadableType, getLayoutFromParcels } from './utils'
import { SceneSourcePlacement } from 'shared/types'

export const BASE_DOWNLOAD_URL = 'https://builder-api.decentraland.org/v1/storage/contents'
const BASE_BUILDER_SERVER_URL_ROPSTEN = 'https://builder-api.decentraland.io/v1/'
export const BASE_BUILDER_SERVER_URL = 'https://builder-api.decentraland.org/v1/'
export const BUILDER_MANIFEST_VERSION = 10

export class BuilderServerAPIManager {
  private static readonly AUTH_CHAIN_HEADER_PREFIX = 'x-identity-auth-chain-'
  private readonly assets: Map<AssetId, BuilderAsset> = new Map()

  static authorize(identity: ExplorerIdentity, method: string = 'get', path: string = '') {
    const headers: Record<string, string> = {}

    if (identity) {
      const endpoint = (method + ':' + path).toLowerCase()
      const authChain = Authenticator.signPayload(identity, endpoint)
      for (let i = 0; i < authChain.length; i++) {
        headers[this.AUTH_CHAIN_HEADER_PREFIX + i] = JSON.stringify(authChain[i])
      }
    }
    return headers
  }

  public addBuilderAssets(assets: BuilderAsset[]) {
    if (assets) {
      try {
        assets.forEach((asset) => this.assets.set(asset.id, asset))
      } catch (e) {
        defaultLogger.error(e)
      }
    }
  }

  async getAssets(assetIds: AssetId[]): Promise<Record<string, BuilderAsset>> {
    const unknownAssets = assetIds.filter((assetId) => !this.assets.has(assetId))
    // TODO: If there are too many assets, we might end up over the url limit, so we might need to send multiple requests
    if (unknownAssets.length > 0) {
      const queryParams = 'assets?id=' + unknownAssets.join('&id=')
      try {
        const url = `${this.getBaseUrl()}${queryParams}`
        // Fetch unknown assets
        const response = await fetch(url)
        const { data }: { data: BuilderAsset[] } = await response.json()
        data.map((builderAsset) => builderAsset).forEach((asset) => this.assets.set(asset.id, asset))
      } catch (e) {
        defaultLogger.error(e)
      }
    }
    const assets: Record<string, BuilderAsset> = {}

    assetIds.map((assetId) => {
      assets[assetId] = this.assets.get(assetId)!
    })
    return assets
  }

  async getBuilderAssets(assetIds: AssetId[]): Promise<BuilderAsset[]> {
    await this.getAssets(assetIds)
    const builderAssets: BuilderAsset[] = []
    assetIds.forEach((assetId) => builderAssets.push(this.assets.get(assetId)!))
    return builderAssets
  }

  async getConvertedAssets(assetIds: AssetId[]): Promise<Map<AssetId, Asset>> {
    await this.getAssets(assetIds)
    return new Map(assetIds.map((assetId) => [assetId, this.builderAssetToLocalAsset(this.assets.get(assetId)!)]))
  }

  async getBuilderManifestFromProjectId(
    projectId: string,
    identity: ExplorerIdentity
  ): Promise<BuilderManifest | undefined> {
    try {
      // Fetch builder manifest by ID
      const queryParams = 'projects/' + projectId + '/manifest'
      const urlToFecth = `${this.getBaseUrl()}${queryParams}`

      let params: RequestInit = {
        headers: BuilderServerAPIManager.authorize(identity, 'get', '/' + queryParams)
      }

      const response = await fetch(urlToFecth, params)
      const data = await response.json()

      const manifest: BuilderManifest = data.data

      // If this manifest contains assets, we add them so we don't need to fetch them
      if (manifest) this.addAssetsFromManifest(manifest)
      return manifest
    } catch (e) {
      defaultLogger.error(e)
      return undefined
    }
  }

  async getBuilderManifestFromLandCoordinates(
    land: string,
    identity: ExplorerIdentity
  ): Promise<BuilderManifest | undefined> {
    try {
      // Fetch builder manifest by lands coordinates
      const queryParams = 'manifests?' + 'creation_coords_eq=' + land
      const urlToFecth = `${this.getBaseUrl()}${queryParams}`

      let params: RequestInit = {
        headers: BuilderServerAPIManager.authorize(identity, 'get', '/' + queryParams)
      }

      const response = await fetch(urlToFecth, params)
      const data = await response.json()

      if (data.data.length === 0) {
        return undefined
      }
      const manifest: BuilderManifest = data.data[0]

      // If this manifest contains assets, we add them so we don't need to fetch them
      if (manifest) this.addAssetsFromManifest(manifest)

      return manifest
    } catch (e) {
      defaultLogger.error(e)
      return undefined
    }
  }

  async updateProjectManifest(builderManifest: BuilderManifest, identity: ExplorerIdentity) {
    try {
      builderManifest.project.updated_at = new Date().toISOString()
      await this.setManifestOnServer(builderManifest, identity)
    } catch (e) {
      defaultLogger.error(e)
    }
  }

  async updateProjectThumbnail(projectId: string, thumbnailBlob: Blob, identity: ExplorerIdentity) {
    try {
      await this.setThumbnailOnServer(projectId, thumbnailBlob, identity)
    } catch (e) {
      defaultLogger.error(e)
    }
  }

  async createProjectWithCoords(coordinates: string, identity: ExplorerIdentity): Promise<BuilderManifest> {
    const builderManifest = this.createEmptyDefaultBuilderScene(coordinates, identity.rawAddress)
    try {
      await this.setManifestOnServer(builderManifest, identity)
    } catch (e) {
      defaultLogger.error(e)
    }
    return builderManifest
  }

  async builderManifestFromSerializedState(
    builderSceneId: string,
    builderProjectId: string,
    baseParcel: string,
    parcels: string[],
    title: string | undefined,
    description: string | undefined,
    ethAddress: string,
    scene: SerializedSceneState,
    sceneLayout: SceneSourcePlacement['layout'] | undefined
  ): Promise<BuilderManifest> {
    const builderProject: BuilderProject = this.createBuilderProject(
      builderSceneId,
      builderProjectId,
      baseParcel,
      parcels,
      ethAddress,
      title,
      description,
      sceneLayout?.cols,
      sceneLayout?.rows
    )

    const { entities, components } = getBuilderEntitiesAndComponentsFromSerializedState(scene)
    const assetsId: string[] = Object.values(components)
      .filter((component) => fromHumanReadableType(component.type) === CLASS_ID.GLTF_SHAPE)
      .map((component) => component.data.assetId)

    const assets = await this.getAssets(assetsId)

    const ground: BuilderGround = Object.values(assets)
      .filter((asset) => asset.category === 'ground')
      ?.map((groundAsset) => {
        return {
          assetId: groundAsset.id,
          componentId: Object.values(components).filter((component) => component.data.assetId === groundAsset.id)[0]?.id
        }
      })[0]

    const groundEntity = Object.values(entities).filter((entity) =>
      entity.components.find((componentId) => componentId === ground.componentId)
    )[0]

    if (groundEntity) {
      groundEntity.disableGizmos = true
    }

    // NOTE: scene metrics are calculated again in builder dapp, so for now we only fill entities count
    let builderScene: BuilderScene = {
      id: builderSceneId,
      entities,
      components,
      assets,
      ground,
      limits: getSceneLimits(parcels.length),
      metrics: {
        textures: 0,
        triangles: 0,
        materials: 0,
        meshes: 0,
        bodies: 0,
        entities: Object.keys(entities).length
      }
    }

    return {
      version: BUILDER_MANIFEST_VERSION,
      project: builderProject,
      scene: builderScene
    }
  }

  private createBuilderProject(
    builderSceneId: string,
    builderProjectId: string,
    baseParcel: string,
    parcels: string[],
    ethAddress: string,
    title?: string,
    description?: string,
    cols?: number,
    rows?: number
  ): BuilderProject {
    const today = new Date().toISOString()

    const layout = getLayoutFromParcels(parcels)

    return {
      id: builderProjectId,
      title: title ?? `Builder ${baseParcel}`,
      description: description ?? 'Scene created from the explorer builder',
      is_public: false,
      scene_id: builderSceneId,
      eth_address: ethAddress,
      rows: rows ?? layout.rows,
      cols: cols ?? layout.cols,
      created_at: today,
      updated_at: today
    }
  }

  private builderAssetToLocalAsset(webAsset: BuilderAsset): Asset {
    return {
      id: webAsset.id,
      model: webAsset.model,
      mappings: Object.entries(webAsset.contents).map(([file, hash]) => ({ file, hash })),
      baseUrl: BASE_DOWNLOAD_URL
    }
  }

  private async setManifestOnServer(builderManifest: BuilderManifest, identity: ExplorerIdentity) {
    const queryParams = 'projects/' + builderManifest.project.id + '/manifest'
    const urlToFecth = `${this.getBaseUrl()}${queryParams}`

    const body = JSON.stringify({ manifest: builderManifest })
    const headers = BuilderServerAPIManager.authorize(identity, 'put', '/' + queryParams)
    headers['Content-Type'] = 'application/json'

    let params: RequestInit = {
      headers: headers,
      method: 'PUT',
      body: body
    }

    const response = await fetch(urlToFecth, params)
    const data = await response.json()
    return data
  }

  private async setThumbnailOnServer(projectId: string, thumbnailBlob: Blob, identity: ExplorerIdentity) {
    const queryParams = 'projects/' + projectId + '/media'
    const urlToFecth = `${this.getBaseUrl()}${queryParams}`

    const thumbnailData = new FormData()
    thumbnailData.append('thumbnail', thumbnailBlob)
    const headers = BuilderServerAPIManager.authorize(identity, 'post', '/' + queryParams)

    let params: RequestInit = {
      headers: headers,
      method: 'POST',
      body: thumbnailData
    }

    const response = await fetch(urlToFecth, params)
    const data = await response.json()
    return data
  }

  private getBaseUrl(): string {
    if (getDefaultTLD() === 'org') return BASE_BUILDER_SERVER_URL
    else return BASE_BUILDER_SERVER_URL_ROPSTEN
  }

  private addAssetsFromManifest(manifest: BuilderManifest) {
    Object.entries(manifest.scene.assets).forEach((asset) => {
      if (!this.assets.has(asset[0])) {
        this.assets.set(asset[0], asset[1])
      }
    })
  }

  private createEmptyDefaultBuilderScene(land: string, ethAddress: string): BuilderManifest {
    let builderSceneId = uuid()
    let builderProjectId = uuid()
    let builderProject = this.createBuilderProject(builderSceneId, builderProjectId, land, [land], ethAddress)
    builderProject.creation_coords = land

    let builderScene: BuilderScene = {
      id: builderSceneId,
      entities: {
        '29d657c1-95cf-4e17-b424-fe252d43ced5': {
          id: '29d657c1-95cf-4e17-b424-fe252d43ced5',
          components: ['14708436-ffd4-44d6-8a28-48d8fcb65917', '47924b6e-27ba-41a3-8bd9-c025cd092a48'],
          disableGizmos: true,
          name: '29d657c1-95cf-4e17-b424-fe252d43ced5'
        }
      },
      components: {
        '14708436-ffd4-44d6-8a28-48d8fcb65917': {
          id: '14708436-ffd4-44d6-8a28-48d8fcb65917',
          type: 'GLTFShape',
          data: {
            assetId: 'c9b17021-765c-4d9a-9966-ce93a9c323d1'
          }
        },
        '47924b6e-27ba-41a3-8bd9-c025cd092a48': {
          id: '47924b6e-27ba-41a3-8bd9-c025cd092a48',
          type: 'Transform',
          data: {
            position: {
              x: 8,
              y: 0,
              z: 8
            },
            rotation: {
              x: 0,
              y: 0,
              z: 0,
              w: 1
            },
            scale: {
              x: 1,
              y: 1,
              z: 1
            }
          }
        }
      },
      assets: {},
      metrics: {
        textures: 0,
        triangles: 0,
        materials: 0,
        meshes: 0,
        bodies: 0,
        entities: 0
      },
      limits: {
        textures: 10,
        triangles: 10000,
        materials: 20,
        meshes: 200,
        bodies: 300,
        entities: 200
      },
      ground: {
        assetId: 'c9b17021-765c-4d9a-9966-ce93a9c323d1',
        componentId: 'b5edf28e-b4e4-4a27-b0ac-84b3d77eff8e'
      }
    }
    let builderManifest: BuilderManifest = {
      version: BUILDER_MANIFEST_VERSION,
      project: builderProject,
      scene: builderScene
    }
    return builderManifest
  }
}

function getSceneLimits(parcelsCount: number): BuilderMetric {
  const sceneLimits = getParcelSceneLimits(parcelsCount)
  return {
    bodies: sceneLimits.bodies,
    entities: sceneLimits.entities,
    materials: sceneLimits.materials,
    textures: sceneLimits.textures,
    triangles: sceneLimits.triangles,
    meshes: sceneLimits.geometries
  }
}

function getBuilderEntitiesAndComponentsFromSerializedState(
  scene: SerializedSceneState
): { entities: Record<string, BuilderEntity>; components: Record<string, BuilderComponent> } {
  let entities: Record<string, BuilderEntity> = {}
  let builderComponents: Record<string, BuilderComponent> = {}

  for (const entity of scene.entities) {
    let builderComponentsIds: string[] = []

    for (const component of entity.components) {
      const newId = uuid()
      builderComponentsIds.push(newId)

      if (component.type === CLASS_ID.GLTF_SHAPE) {
        component.value.url = component.value.src
      }

      let builderComponent: BuilderComponent = {
        id: newId,
        type: toHumanReadableType(component.type),
        data: component.value
      }
      builderComponents[builderComponent.id] = builderComponent
    }

    let builderEntity: BuilderEntity = {
      id: entity.id,
      components: builderComponentsIds,
      disableGizmos: false,
      name: entity.id
    }
    entities[builderEntity.id] = builderEntity
  }
  return { entities, components: builderComponents }
}
