import { Store } from 'redux'
import { Authenticator } from 'dcl-crypto'
import { ContentClient } from 'dcl-catalyst-client'
import { EntityType } from 'dcl-catalyst-commons'
import { getUpdateProfileServer } from 'shared/dao/selectors'
import { RootState } from 'shared/store/rootTypes'
import { getCurrentIdentity } from 'shared/session/selectors'
import { DeploymentResult, CONTENT_PATH, SceneDeploymentSourceMetadata } from './types'
import { defaultLogger } from '../../logger'
import { ContentMapping, SceneJsonData } from '../../types'
import { jsonFetch } from '../../../atomicHelpers/jsonFetch'

declare const globalThis: any

declare type SceneDeployment = {
  sceneJson: SceneJsonData
  sceneFiles: Map<string, Buffer>
}

export async function unpublishSceneByCoords(coordinates: string): Promise<DeploymentResult> {
  let result

  try {
    const { sceneJson, sceneFiles } = await getEmptySceneFiles(coordinates)
    debugger
    // NOTE: await deploy(sceneData.sceneJsonData.scene.base, scene)

    const contentClient = getContentClient()

    const { files, entityId } = await contentClient.buildEntity({
      type: EntityType.SCENE,
      pointers: [coordinates],
      files: sceneFiles,
      metadata: {
        ...sceneJson,
        source: {
          origin: 'builder-in-world',
          version: 1,
          isEmpty: true
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

    debugger
    result = { ok: true, error: '' }
  } catch (error) {
    result = { ok: false, error: `Unpublish failed ${error}` }
    defaultLogger.error('Unpublish failed', error)
  }

  return result
}

async function getEmptySceneFiles(coordinates: string): Promise<SceneDeployment> {
  const fullRootUrl =
    `${location.protocol}//${location.host}${location.pathname}`.replace('index.html', '') + 'loader/empty-scenes/'

  const scenes = await jsonFetch(fullRootUrl + 'mappings.json')
  const scenesContents: ContentMapping[][] = Object.values(scenes)
  const scenesNames: string[] = Object.keys(scenes)
  const randomSceneIndex: number = Math.floor(Math.random() * scenesContents.length)

  const emptySceneName: string = scenesNames[randomSceneIndex]
  const emptySceneBaseUrl: string = fullRootUrl + emptySceneName
  const emptySceneMappings: ContentMapping[] = scenesContents[randomSceneIndex]
  const emptySceneJsonFile: string | undefined = emptySceneMappings.find(
    (content) => content.file === CONTENT_PATH.SCENE_FILE
  )?.hash
  const emptySceneGameFile: string | undefined = emptySceneMappings.find(
    (content) => content.file === CONTENT_PATH.BUNDLED_GAME_FILE
  )?.file

  if (!emptySceneJsonFile) {
    throw Error(`empty-scene ${CONTENT_PATH.SCENE_FILE} file not found`)
  }

  if (!emptySceneGameFile) {
    throw Error(`empty-scene ${CONTENT_PATH.BUNDLED_GAME_FILE} file not found`)
  }

  const newSceneJson: SceneJsonData = await (await fetch(`${emptySceneBaseUrl}/${emptySceneJsonFile}`)).json()
  newSceneJson.scene.parcels = [coordinates]
  newSceneJson.scene.base = coordinates

  const newSceneGameJS = await (await fetch(`${emptySceneBaseUrl}/${emptySceneGameFile}`)).text()
  const newSceneModels = await getModelsFiles(emptySceneBaseUrl, emptySceneMappings)

  const entityFiles: Map<string, Buffer> = new Map([
    [CONTENT_PATH.BUNDLED_GAME_FILE, Buffer.from(newSceneGameJS)],
    [CONTENT_PATH.SCENE_FILE, Buffer.from(JSON.stringify(newSceneJson))],
    ...newSceneModels
  ])

  return { sceneJson: newSceneJson, sceneFiles: entityFiles }
}

async function getModelsFiles(baseUrl: string, mappings: ContentMapping[]) {
  const assets = mappings.filter(
    (mapping) => mapping.file !== CONTENT_PATH.SCENE_FILE && mapping.file !== CONTENT_PATH.BUNDLED_GAME_FILE
  )

  const promises: Promise<[string, Buffer]>[] = assets.map<Promise<[string, Buffer]>>(async (asset) => {
    const response = await fetch(`${baseUrl}/${asset.hash}`)
    const blob = await response.blob()
    const buffer = await blobToBuffer(blob)
    return [asset.file, buffer]
  })

  const result = await Promise.all(promises)
  return new Map(result)
}

// async function deploy(baseParcel: string, scene: SceneDeployment) {
//   const contentClient = getContentClient()

//   const { files, entityId } = await contentClient.buildEntity({
//     type: EntityType.SCENE,
//     pointers: [baseParcel],
//     files: scene.sceneFiles,
//     metadata: {
//       ...scene.sceneJson,
//       source: {
//         origin: 'builder-in-world',
//         version: 1,
//         isEmpty: true
//       } as SceneDeploymentSourceMetadata
//     }
//   })

//   // Sign entity id
//   const store: Store<RootState> = globalThis['globalStore']
//   const identity = getCurrentIdentity(store.getState())
//   if (!identity) {
//     throw new Error('Identity not found when trying to deploy an entity')
//   }
//   const authChain = Authenticator.signPayload(identity, entityId)

//   await contentClient.deployEntity({ files, entityId, authChain })
// }

// NOTE: duplicated
const toBuffer = require('blob-to-buffer')
function blobToBuffer(blob: Blob): Promise<Buffer> {
  return new Promise((resolve, reject) => {
    toBuffer(blob, (err: Error, buffer: Buffer) => {
      if (err) reject(err)
      resolve(buffer)
    })
  })
}

function getContentClient(): ContentClient {
  const store: Store<RootState> = globalThis['globalStore']
  const contentUrl = getUpdateProfileServer(store.getState())
  return new ContentClient(contentUrl, 'builder in-world')
}
