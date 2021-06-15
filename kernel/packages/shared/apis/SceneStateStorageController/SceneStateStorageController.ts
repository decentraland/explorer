import { exposeMethod, setAPIName } from 'decentraland-rpc/lib/host'
import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { ContentClient } from 'dcl-catalyst-client'
import { EntityType, Pointer, ContentFileHash } from 'dcl-catalyst-commons'
import { Authenticator } from 'dcl-crypto'
import { ExposableAPI } from '../ExposableAPI'
import { defaultLogger } from '../../logger'
import { DEBUG } from '../../../config'
import {
  Asset,
  AssetId,
  BuilderManifest,
  CONTENT_PATH,
  DeploymentResult,
  SceneDeploymentSourceMetadata,
  SerializedSceneState
} from './types'
import { getCurrentIdentity } from 'shared/session/selectors'
import { BuilderServerAPIManager } from './BuilderServerAPIManager'
import {
  fromBuildertoStateDefinitionFormat,
  fromSerializedStateToStorableFormat,
  fromStorableFormatToSerializedState,
  StorableSceneState,
  toBuilderFromStateDefinitionFormat
} from './StorableSceneStateTranslation'
import { CLASS_ID, uuid } from 'decentraland-ecs/src'
import { ParcelIdentity } from '../ParcelIdentity'
import { Store } from 'redux'
import { RootState } from 'shared/store/rootTypes'
import { getUpdateProfileServer } from 'shared/dao/selectors'
import { createGameFile } from './SceneStateDefinitionCodeGenerator'
import { SceneStateDefinition } from 'scene-system/stateful-scene/SceneStateDefinition'
import { ExplorerIdentity } from 'shared/session/types'
import { deserializeSceneState, serializeSceneState } from 'scene-system/stateful-scene/SceneStateDefinitionSerializer'
import { ISceneStateStorageController } from './ISceneStateStorageController'
import { base64ToBlob } from 'atomicHelpers/base64ToBlob'

declare const globalThis: any

export class SceneStateStorageController extends ExposableAPI implements ISceneStateStorageController {
  private readonly builderApiManager = new BuilderServerAPIManager()
  private parcelIdentity = this.options.getAPIInstance(ParcelIdentity)
  private builderManifest!: BuilderManifest

  @exposeMethod
  async getProjectManifest(projectId: string): Promise<SerializedSceneState | undefined> {
    const manifest = await this.builderApiManager.getBuilderManifestFromProjectId(projectId, this.getIdentity())

    if (!manifest) return undefined

    globalThis.unityInterface.SendBuilderProjectInfo(manifest.project.title, manifest.project.description)
    this.builderManifest = manifest
    const definition = fromBuildertoStateDefinitionFormat(manifest.scene)
    return serializeSceneState(definition)
  }

  @exposeMethod
  async getProjectManifestByCoordinates(land: string): Promise<SerializedSceneState | undefined> {
    const newProject = await this.builderApiManager.getBuilderManifestFromLandCoordinates(land, this.getIdentity())
    if (newProject) {
      globalThis.unityInterface.SendBuilderProjectInfo(newProject.project.title, newProject.project.description)
      this.builderManifest = newProject
      const translatedManifest = fromBuildertoStateDefinitionFormat(this.builderManifest.scene)
      return serializeSceneState(translatedManifest)
    }
    return undefined
  }

  @exposeMethod
  async createProjectWithCoords(coordinates: string): Promise<boolean> {
    const newProject = await this.builderApiManager.createProjectWithCoords(coordinates, this.getIdentity())
    globalThis.unityInterface.SendBuilderProjectInfo(newProject.project.title, newProject.project.description)
    this.builderManifest = newProject
    return newProject ? true : false
  }

  @exposeMethod
  async saveSceneState(serializedSceneState: SerializedSceneState): Promise<DeploymentResult> {
    let result: DeploymentResult

    try {
      // Deserialize the scene state
      const sceneState: SceneStateDefinition = deserializeSceneState(serializedSceneState)

      // Convert the scene state to builder scheme format
      let builderManifest = await toBuilderFromStateDefinitionFormat(
        sceneState,
        this.builderManifest,
        this.builderApiManager
      )

      // Update the manifest
      await this.builderApiManager.updateProjectManifest(builderManifest, this.getIdentity())
      result = { ok: true }
    } catch (error) {
      defaultLogger.error('Saving manifest failed', error)
      result = { ok: false, error: `${error}` }
    }
    return result
  }

  @exposeMethod
  async publishSceneState(
    sceneId: string,
    sceneName: string,
    sceneDescription: string,
    sceneScreenshot: string,
    sceneState: SerializedSceneState
  ): Promise<DeploymentResult> {
    let result: DeploymentResult

    // Deserialize the scene state
    const deserializedSceneState: SceneStateDefinition = deserializeSceneState(sceneState)

    // Convert the scene state to builder scheme format
    let builderManifest = await toBuilderFromStateDefinitionFormat(
      deserializedSceneState,
      this.builderManifest,
      this.builderApiManager
    )

    // Update the project info
    builderManifest.project.title = sceneName
    builderManifest.project.description = sceneDescription

    // Update the manifest
    await this.builderApiManager.updateProjectManifest(builderManifest, this.getIdentity())

    // Update the thumbnail
    const thumbnailBlob: Blob = base64ToBlob(sceneScreenshot, 'image/png')
    await this.builderApiManager.updateProjectThumbnail(builderManifest.project.id, thumbnailBlob, this.getIdentity())

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
        sceneJson.display = {
          title: sceneName,
          description: sceneDescription,
          navmapThumbnail: CONTENT_PATH.SCENE_THUMBNAIL
        }

        // Group all entity files
        const entityFiles: Map<string, Buffer> = new Map([
          [CONTENT_PATH.DEFINITION_FILE, Buffer.from(JSON.stringify(storableFormat))],
          [CONTENT_PATH.BUNDLED_GAME_FILE, Buffer.from(gameFile)],
          [CONTENT_PATH.SCENE_FILE, Buffer.from(JSON.stringify(sceneJson))],
          [CONTENT_PATH.SCENE_THUMBNAIL, await blobToBuffer(thumbnailBlob)],
          ...models
        ])

        // Deploy
        const contentClient = this.getContentClient()

        // Build the entity
        const parcels = this.getParcels()
        const { files, entityId } = await contentClient.buildEntity({
          type: EntityType.SCENE,
          pointers: parcels,
          files: entityFiles,
          metadata: {
            ...sceneJson,
            source: {
              origin: 'builder-in-world',
              version: 1,
              projectId: this.builderManifest.project.id
            } as SceneDeploymentSourceMetadata
          }
        })

        // Sign entity id
        const store: Store<RootState> = globalThis['globalStore']
        const identity = getCurrentIdentity(store.getState())
        if (!identity) {
          throw new Error('Identity not found when trying to deploy an entity')
        }
        const authChain = Authenticator.signPayload(identity, entityId)

        await contentClient.deployEntity({ files, entityId, authChain })

        result = { ok: true }
      } catch (error) {
        defaultLogger.error('Deployment failed', error)
        result = { ok: false, error: `${error}` }
      }
    }
    globalThis.unityInterface.SendPublishSceneResult(result)
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
      // NOTE: RPC controllers should NEVER return undefined. Use null instead
      return undefined
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

  @exposeMethod
  async createProjectFromStateDefinition(): Promise<SerializedSceneState | undefined> {
    const sceneJson = this.parcelIdentity.land.sceneJsonData
    const sceneId: string = this.parcelIdentity.land.sceneId
    const baseParcel: string = sceneJson.scene.base
    const parcels: string[] = sceneJson.scene.parcels
    const title: string | undefined = sceneJson.display?.title
    const description: string | undefined = sceneJson.display?.description
    const thumbnailHash: string | undefined = this.parcelIdentity.land.mappingsResponse.contents.find(
      (pair) => pair.file === CONTENT_PATH.SCENE_THUMBNAIL
    )?.hash

    try {
      const serializedScene = await this.getStoredState(sceneId)
      if (serializedScene) {
        const identity = this.getIdentity()

        let builderManifest = await this.builderApiManager.createManifestFromSerializedState(
          uuid(),
          uuid(),
          baseParcel,
          parcels,
          title,
          description,
          serializedScene,
          identity
        )
        if (builderManifest) {
          this.builderManifest = builderManifest
          globalThis.unityInterface.SendBuilderProjectInfo(
            builderManifest.project.title,
            builderManifest.project.description
          )

          let thumbnail: string = ''

          if (thumbnailHash) {
            const contentClient = this.getContentClient()
            const thumbnailBuffer = await contentClient.downloadContent(thumbnailHash, { attempts: 3 })
            thumbnail = thumbnailBuffer.toString('base64')
          }

          this.publishSceneState(
            sceneId,
            builderManifest.project.title,
            builderManifest.project.description,
            thumbnail,
            serializedScene
          ).catch((error) => defaultLogger.error(`Error updating project manifest ${error}`))

          return serializedScene
        }
      }
    } catch (error) {
      defaultLogger.error(`Failed creating project from state definition at coords ${baseParcel}`)
    }
  }

  private getIdentity(): ExplorerIdentity {
    const store: Store<RootState> = globalThis['globalStore']
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
    const store: Store<RootState> = globalThis['globalStore']
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
setAPIName('SceneStateStorageController', SceneStateStorageController)

const toBuffer = require('blob-to-buffer')
export function blobToBuffer(blob: Blob): Promise<Buffer> {
  return new Promise((resolve, reject) => {
    toBuffer(blob, (err: Error, buffer: Buffer) => {
      if (err) reject(err)
      resolve(buffer)
    })
  })
}
