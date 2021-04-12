import { exposeMethod, registerAPI } from 'decentraland-rpc/lib/host'
import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { ContentClient, DeploymentBuilder } from 'dcl-catalyst-client'
import { EntityType, Pointer, ContentFileHash } from 'dcl-catalyst-commons'
import { Authenticator } from 'dcl-crypto'
import { ExposableAPI } from '../ExposableAPI'
import { defaultLogger } from '../../logger'
import { DEBUG } from '../../../config'
import { BuilderManifest, CONTENT_PATH, DeploymentResult, SerializedSceneState } from './types'
import { getCurrentIdentity } from 'shared/session/selectors'
import { Asset, AssetId, BuilderServerAPIManager } from './BuilderServerAPIManager'
import {
  fromBuildertoStateDefinitionFormat,
  fromSerializedStateToStorableFormat,
  fromStorableFormatToSerializedState,
  StorableSceneState,
  toBuilderFromStateDefinitionFormat
} from './StorableSceneStateTranslation'
import { CLASS_ID } from 'decentraland-ecs/src'
import { ParcelIdentity } from '../ParcelIdentity'
import { Store } from 'redux'
import { RootState } from 'shared/store/rootTypes'
import { getUpdateProfileServer } from 'shared/dao/selectors'
import { createGameFile } from './SceneStateDefinitionCodeGenerator'
import { SceneStateDefinition } from 'scene-system/stateful-scene/SceneStateDefinition'
import { ILand } from 'shared/types'
import { ExplorerIdentity } from 'shared/session/types'
import { deserializeSceneState, serializeSceneState } from 'scene-system/stateful-scene/SceneStateDefinitionSerializer'

declare const window: any

@registerAPI('SceneStateStorageController')
export class SceneStateStorageController extends ExposableAPI {
  private builderApiManager = new BuilderServerAPIManager()
  private parcelIdentity = this.options.getAPIInstance(ParcelIdentity)
  private builderManifest!: BuilderManifest

  @exposeMethod
  async getProjectManifest(projectId: string): Promise<SerializedSceneState | undefined> {
    const manifest = await this.builderApiManager.getBuilderManifestFromProjectId(projectId, this.getIdentity())

    if (!manifest) return undefined

    this.builderManifest = manifest
    const definition = fromBuildertoStateDefinitionFormat(manifest.scene)
    return serializeSceneState(definition)
  }

  @exposeMethod
  async getProjectManifestByCoordinates(land: string): Promise<SerializedSceneState | undefined> {
    const newProject = await this.builderApiManager.getBuilderManifestFromLandCoordinates(land, this.getIdentity())
    if (newProject) {
      this.builderManifest = newProject
      const translatedManifest = fromBuildertoStateDefinitionFormat(this.builderManifest.scene)
      return serializeSceneState(translatedManifest)
    }
    return undefined
  }

  @exposeMethod
  async createProjectWithCoords(land: ILand): Promise<SerializedSceneState> {
    const newProject = await this.builderApiManager.createProjectWithCoords(land, this.getIdentity())
    this.builderManifest = newProject   
    return serializeSceneState(new SceneStateDefinition())
  }

  @exposeMethod
  async saveProjectManifest(serializedSceneState: SerializedSceneState) {
    //Deserialize the scene state
    const sceneState: SceneStateDefinition = deserializeSceneState(serializedSceneState)

    //Convert the scene state to builder scheme format
    let builderManifest = toBuilderFromStateDefinitionFormat(sceneState, this.builderManifest)

    //We get all the assetIds from the gltfShapes so we can fetch the corresponded asset
    let idArray: string[] = []
    Object.entries(builderManifest.scene.components).forEach((component) => {
      if (component[1].type === 'GLTFShape') {
        let found = false
        Object.entries(builderManifest.scene.assets).forEach((assets) => {
          if (assets[0] === component[1].data.assetId) {
            found = true
          }
        })
        if (!found) {
          idArray.push(component[1].data.assetId)
        }
      }
    })

    //We fetch all the assets that the scene contains since builder needs the assets
    builderManifest.scene.assets = await this.builderApiManager.getAssets(idArray)

    //This is a special case. The builder needs the ground separated from the rest of the components so we search for it.
    //Unity handles this, so only 1 entitty will contain the "ground" category. We can safely assume that we can search it and assign
    Object.entries(builderManifest.scene.assets).forEach((asset) => {
      if (asset[1].category === 'ground') {
        builderManifest.scene.ground.assetId = asset[0]
        Object.entries(builderManifest.scene.components).forEach((component) => {
          if (component[1].data.assetId === asset[0]) builderManifest.scene.ground.componentId = component[0]
        })
      }
    })

    //Update the manifest
    this.builderApiManager.updateProjectManifest(builderManifest, this.getIdentity())
  }

  @exposeMethod
  async storeState(sceneId: string, sceneState: SerializedSceneState): Promise<DeploymentResult> {
    let result: DeploymentResult

    // Convert to storable format
    const storableFormat = fromSerializedStateToStorableFormat(sceneState)

    if (DEBUG) {
      saveToLocalStorage(`scene-state-${sceneId}`, storableFormat)
      result = { ok: true }
    } else {
      try {
        // Fetch all asset metadata
        const assets = await this.getAllAssets(sceneState)

        // Download asset files
        const models = await this.downloadAssetFiles(assets)

        // Generate game file
        const gameFile: string = createGameFile(sceneState, assets)

        // Prepare scene.json
        const sceneJson = this.parcelIdentity.land.sceneJsonData

        // Group all entity files
        const entityFiles: Map<string, Buffer> = new Map([
          [CONTENT_PATH.DEFINITION_FILE, Buffer.from(JSON.stringify(storableFormat))],
          [CONTENT_PATH.BUNDLED_GAME_FILE, Buffer.from(gameFile)],
          [CONTENT_PATH.SCENE_FILE, Buffer.from(JSON.stringify(sceneJson))],
          ...models
        ])

        // Build the entity
        const parcels = this.getParcels()
        const { files, entityId } = await DeploymentBuilder.buildEntity(
          EntityType.SCENE,
          parcels,
          entityFiles,
          sceneJson,
          Date.now()
        )

        // Sign entity id
        const authChain = Authenticator.signPayload(this.getIdentity(), entityId)

        // Deploy
        const contentClient = this.getContentClient()
        await contentClient.deployEntity({ files, entityId, authChain })

        result = { ok: true }
      } catch (error) {
        defaultLogger.error('Deployment failed', error)
        result = { ok: false, error: `${error}` }
      }
    }
    window.unityInterface.SendPublishSceneResult(result)
    return result
  }

  @exposeMethod
  async getStoredState(sceneId: string): Promise<SerializedSceneState | undefined> {
    if (DEBUG) {
      const sceneState: StorableSceneState = getFromLocalStorage(`scene-state-${sceneId}`)
      if (sceneState) {
        return fromStorableFormatToSerializedState(sceneState)
      }
      defaultLogger.warn(`Couldn't find a local scene state for scene ${sceneId}`)
      return
    }

    const contentClient = this.getContentClient()
    try {
      // Fetch the entity and find the definition's hash
      const scene = await contentClient.fetchEntityById(EntityType.SCENE, this.parcelIdentity.cid, { attempts: 3 })
      const definitionHash: ContentFileHash | undefined = scene.content?.find(
        ({ file }) => file === CONTENT_PATH.DEFINITION_FILE
      )?.hash

      if (definitionHash) {
        // Download the definition and return it
        const definitionBuffer = await contentClient.downloadContent(definitionHash, { attempts: 3 })
        const definitionFile = JSON.parse(definitionBuffer.toString())
        return fromStorableFormatToSerializedState(definitionFile)
      } else {
        defaultLogger.warn(
          `Couldn't find a definition file on the content server for the current scene (${this.parcelIdentity.cid})`
        )
      }
    } catch (e) {
      defaultLogger.error(`Failed to fetch the current scene (${this.parcelIdentity.cid}) from the content server`, e)
    }
  }

  private getIdentity(): ExplorerIdentity {
    const store: Store<RootState> = window['globalStore']
    const identity = getCurrentIdentity(store.getState())
    if (!identity) {
      throw new Error('Identity not found when trying to deploy an entity')
    }
    return identity
  }

  private getParcels(): Pointer[] {
    return this.parcelIdentity.land.sceneJsonData.scene.parcels
  }

  private getContentClient(): ContentClient {
    const store: Store<RootState> = window['globalStore']
    const contentUrl = getUpdateProfileServer(store.getState())
    return new ContentClient(contentUrl, 'builder in-world')
  }

  private getAllAssets(state: SerializedSceneState): Promise<Map<AssetId, Asset>> {
    const assetIds: Set<AssetId> = new Set()
    for (const entity of state.entities) {
      entity.components
        .filter(({ type, value }) => type === CLASS_ID.GLTF_SHAPE && value.assetId)
        .forEach(({ value }) => assetIds.add(value.assetId))
    }
    return this.builderApiManager.getConvertedAssets([...assetIds])
  }

  private async downloadAssetFiles(assets: Map<AssetId, Asset>): Promise<Map<string, Buffer>> {
    // Path to url map
    const allMappings: Map<string, string> = new Map()

    // Gather all mappings together
    for (const asset of assets.values()) {
      asset.mappings.forEach(({ file, hash }) =>
        allMappings.set(`${CONTENT_PATH.MODELS_FOLDER}/${file}`, `${asset.baseUrl}/${hash}`)
      )
    }

    // Download models
    const promises: Promise<[string, Buffer]>[] = Array.from(allMappings.entries()).map<Promise<[string, Buffer]>>(
      async ([path, url]) => {
        const response = await fetch(url)
        const blob = await response.blob()
        const buffer = await blobToBuffer(blob)
        return [path, buffer]
      }
    )

    const result = await Promise.all(promises)
    return new Map(result)
  }
}

const toBuffer = require('blob-to-buffer')
function blobToBuffer(blob: Blob): Promise<Buffer> {
  return new Promise((resolve, reject) => {
    toBuffer(blob, (err: Error, buffer: Buffer) => {
      if (err) reject(err)
      resolve(buffer)
    })
  })
}
