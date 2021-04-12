import { Authenticator } from 'dcl-crypto'
import { ExplorerIdentity } from 'shared/session/types'
import { uuid } from 'decentraland-ecs/src/ecs/helpers'
import { ContentMapping, ILand } from '../../types'
import { BuilderAsset, BuilderManifest, BuilderProject, BuilderScene } from './types'

export type AssetId = string

export type Asset = {
  id: AssetId
  model: string
  mappings: ContentMapping[]
  baseUrl: string
}

declare const window: any

const BASE_DOWNLOAD_URL = 'https://builder-api.decentraland.org/v1/storage/contents'
const BASE_ASSET_URL = 'https://builder-api.decentraland.org/v1/assets'
//const BASE_PROJECT_URL = 'http://localhost:5000/v1/projects/'
const BASE_PROJECT_URL = 'https://builder-api.decentraland.io/v1/projects/'


export class BuilderServerAPIManager {
  private readonly fullAssets: Map<AssetId, BuilderAsset> = new Map()

  async getFullAssets(assetIds: AssetId[]): Promise<Record<string, BuilderAsset>> {
    const unknownAssets = assetIds.filter((assetId) => !this.fullAssets.has(assetId))
    // TODO: If there are too many assets, we might end up over the url limit, so we might need to send multiple requests
    if(unknownAssets.length > 0){
    const queryParams = 'id=' + unknownAssets.join('&id=')
    try {
      // Fetch unknown assets
      const response = await fetch(`${BASE_ASSET_URL}?${queryParams}`)
      const { data }: { data: BuilderAsset[] } = await response.json()
      data.map( builderAsset => builderAsset).forEach((asset) => this.fullAssets.set(asset.id, asset))
    } catch (e) {
      console.trace(e)
    }
  }
  const test : Record<string,BuilderAsset> = { }

  assetIds.map((assetId) => {
    test[assetId] = this.fullAssets.get(assetId)!
  })
  return test
  }

  async getConvertedAssets(assetIds: AssetId[]): Promise<Map<AssetId, Asset>> {
    await this.getFullAssets(assetIds)
    return new Map(assetIds.map((assetId) => [assetId, this.webAssetToLocalAsset(this.fullAssets.get(assetId)!)]))
  }

  private webAssetToLocalAsset(webAsset: BuilderAsset): Asset {
    return {
      id: webAsset.id,
      model: webAsset.model,
      mappings: Object.entries(webAsset.contents).map(([file, hash]) => ({ file, hash })),
      baseUrl: BASE_DOWNLOAD_URL
    }
  }

  async getBuilderManifestFromProjectId(projectId: string, identity: ExplorerIdentity): Promise<BuilderManifest | undefined> {

    try {
      // Fetch unknown assets
      const queryParams = projectId + '/manifest'
      const urlToFecth = `${BASE_PROJECT_URL}${queryParams}`

      let params: RequestInit = {
        headers: this.authorize(identity,'get', '/projects/'+queryParams),
      }

      console.log('Cogiendo manifest de url ' + urlToFecth)
      const response = await fetch(urlToFecth,params)
      const data = await response.json()
      console.log('Respuesta del manifest ' + JSON.stringify(data))
      var value = JSON.parse(JSON.stringify(data))//['data']['items'][0]['id']
      const manifest : BuilderManifest = value.data

      console.log('El manifest ' + JSON.stringify(manifest))
      this.addAssetsFromManifest(manifest)
      return manifest
    } catch (e) {
      console.trace (e)
      return undefined
    }
  }

  async getBuilderManifestFromLandCoordinates(land: string, identity: ExplorerIdentity): Promise<BuilderManifest | undefined> {

    try {
      // Fetch unknown assets
      const queryParams = land + '/manifestFromCoordinates'
      const urlToFecth = `${BASE_PROJECT_URL}${queryParams}`

      let params: RequestInit = {
        headers: this.authorize(identity,'get', '/projects/'+queryParams),
      }

      console.log('Cogiendo manifest de coordenadas ' + urlToFecth)
      const response = await fetch(urlToFecth,params)
      const data = await response.json()
      console.log('Respuesta del manifest por coordenadas ' + JSON.stringify(data))
      var value = JSON.parse(JSON.stringify(data))//['data']['items'][0]['id']
      if(value['data'] === 'false')
      {
        return undefined
      }
      const manifest : BuilderManifest = value.data
      this.addAssetsFromManifest(manifest)
      console.log('Manifest de coordendas' + JSON.stringify(manifest))
      return manifest
    } catch (e) {
      console.trace (e)
      return undefined
    }
  }

  async updateProjectManifest(builderManifest: BuilderManifest, identity: ExplorerIdentity) {
    try {
      this.setManifestOnServer(builderManifest,identity)
    } catch (e) {
      console.trace (e)
      return undefined
    }
  }

  async createProjectWithCoords(land: ILand, identity: ExplorerIdentity): Promise<BuilderManifest | undefined> {  
    try {
      var builderManifest = this.createEmptyDefaultBuilderScene(land.sceneJsonData.scene.base, identity.rawAddress)
      this.setManifestOnServer(builderManifest,identity)
      return builderManifest
    } catch (e) {
      console.trace(e)
      return undefined
    }
  }

  async setManifestOnServer(builderManifest: BuilderManifest, identity: ExplorerIdentity)
  {
    const queryParams = builderManifest.project.id + '/manifest'
    const urlToFecth = `${BASE_PROJECT_URL}${queryParams}`

    const body = JSON.stringify({'manifest': builderManifest})
    const headers = this.authorize(identity,'put', '/projects/'+queryParams)
    headers['Content-Type'] = "application/json"

    let params: RequestInit = {
      headers: headers,
      method: "PUT",
      body:  body
    }

    console.log('Setting manifest url' + urlToFecth)
    console.log('Setting manifest ' + body)
    const response = await fetch(urlToFecth,params)
    const data = await response.json()

    console.log('Answer of setting manifest ' + JSON.stringify(data))
    return data
  }

  addAssetsFromManifest(manifest: BuilderManifest)
  {
    Object.entries(manifest.scene.assets).forEach((asset) => {
      if(!this.fullAssets.has(asset[0])){ 
        this.fullAssets.set(asset[0], asset[1])
      }
    })
  }

  createEmptyDefaultBuilderScene(land: string, eth_address: string): BuilderManifest{

    let today = new Date().toISOString().slice(0, 10)
    let projectId = uuid()
    console.log('Creating new project with id ' + projectId)
    let sceneId = uuid()
    let builderProject: BuilderProject = {
      id : projectId,
      title : 'Builder '+land,
      description : 'Scene created from the explorer builder',
      is_public: false,
      scene_id: sceneId,
      eth_address: eth_address,
      rows : 1,
      cols : 1,
      created_at : today,
      updated_at : today,
      created_location: land
    }

    let builderScene: BuilderScene = {
      id : sceneId,
      entities : {},
      components : {},
      assets : {},
      metrics : {
        textures: 0,
        triangles: 0,
        materials: 0,
        meshes: 0,
        bodies: 0,
        entities: 0
    },
    limits : {
        textures: 10,
        triangles: 10000,
        materials: 20,
        meshes: 200,
        bodies: 300,
        entities: 200
    },
    ground :{
        assetId: 'da1fed3c954172146414a66adfa134f7a5e1cb49c902713481bf2fe94180c2cf',
        componentId:'b5edf28e-b4e4-4a27-b0ac-84b3d77eff8e'
    }
    }
    let builderManifest: BuilderManifest = {
      version : 10,
      project : builderProject,
      scene : builderScene
    }
    return builderManifest
  }
    
  private readonly AUTH_CHAIN_HEADER_PREFIX = 'x-identity-auth-chain-'

  createHeaders(idToken: string) {
    if (!idToken) return {}
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${idToken}`
    }
    return headers
  }

  authorize = (identity: ExplorerIdentity, method: string = 'get', path: string = '', ) => {
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
}
