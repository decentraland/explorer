import { gridToWorld } from 'atomicHelpers/parcelScenePositions'
import {
  DEBUG,
  EDITOR,
  ENGINE_DEBUG_PANEL,
  RESET_TUTORIAL,
  SCENE_DEBUG_PANEL,
  SHOW_FPS_COUNTER,
  tutorialEnabled
} from 'config'
import { ReadOnlyVector3 } from 'decentraland-ecs/src/decentraland/math'
import { IEventNames, IEvents, MinimapSceneInfo, ProfileForRenderer } from 'decentraland-ecs/src/decentraland/Types'
import { EventDispatcher } from 'decentraland-rpc/lib/common/core/EventDispatcher'
import { Empty } from 'google-protobuf/google/protobuf/empty_pb'
import { AirdropInfo } from 'shared/airdrops/interface'
import { DevTools } from 'shared/apis/DevTools'
import { ParcelIdentity } from 'shared/apis/ParcelIdentity'
import { globalDCL } from 'shared/globalDCL'
import { unityClientLoaded } from 'shared/loading/types'
import { createLogger, defaultLogger, ILogger } from 'shared/logger'
import { Wearable } from 'shared/profiles/types'
import { builderInterfaceType } from 'shared/renderer-interface/builder/builderInterface'
import { rendererInterfaceType } from 'shared/renderer-interface/rendererInterface/rendererInterfaceType'
import { AttachEntityComponentPayload, ChatMessage, ComponentCreatedPayload, ComponentDisposedPayload, ComponentRemovedPayload, ComponentUpdatedPayload, CreateEntityPayload, EntityAction, EnvironmentData, FriendshipUpdateStatusMessage, FriendsInitializationMessage, HUDConfiguration, HUDElementID, InstancedSpawnPoint, LoadableParcelScene, Notification, OpenNFTDialogPayload, QueryPayload, RemoveEntityPayload, SetEntityParentPayload, UpdateEntityComponentPayload, UpdateUserStatusMessage } from 'shared/types'
import { ParcelSceneAPI } from 'shared/world/ParcelSceneAPI'
import {
  getParcelSceneID} from 'shared/world/parcelSceneManager'
import { SceneWorker } from 'shared/world/SceneWorker'
import { TeleportController } from 'shared/world/TeleportController'
import {
  PB_AttachEntityComponent,
  PB_ComponentCreated,
  PB_ComponentRemoved,
  PB_ComponentUpdated,
  PB_CreateEntity,
  PB_OpenExternalUrl,
  PB_OpenNFTDialog, PB_Query,
  PB_RemoveEntity,
  PB_SendSceneMessage,
  PB_SetEntityParent,
  PB_UpdateEntityComponent
} from '../shared/proto/engineinterface_pb'
import { attachEntity, componentCreated, componentDisposed, componentUpdated, createEntity, direction, openExternalUrl, openNFTDialog, origin, query, ray, rayQuery, removeEntity, removeEntityComponent, setEntityParent, updateEntityComponent } from './cachedProtobuf'
import { initializeDecentralandUI } from './initializeDecentralandUI'
import { setupPosition } from './position/setupPosition'
import { setupPointerLock } from './setupPointerLock'
import { browserInterface } from './browserInterface'

type GameInstance = {
  SendMessage(object: string, method: string, ...args: (number | string)[]): void
}

const rendererVersion = require('decentraland-renderer')
window['console'].log('Renderer version: ' + rendererVersion)

let gameInstance!: GameInstance

globalDCL.browserInterface = browserInterface

const CHUNK_SIZE = 100

export const unityInterface: rendererInterfaceType & builderInterfaceType = {
  debug: false,

  SendGenericMessage(object: string, method: string, payload: string) {
    gameInstance.SendMessage(object, method, payload)
  },
  SetDebug() {
    gameInstance.SendMessage('SceneController', 'SetDebug')
  },
  LoadProfile(profile: ProfileForRenderer) {
    gameInstance.SendMessage('SceneController', 'LoadProfile', JSON.stringify(profile))
  },
  CreateUIScene(data: { id: string; baseUrl: string }) {
    /**
     * UI Scenes are scenes that does not check any limit or boundary. The
     * position is fixed at 0,0 and they are universe-wide. An example of this
     * kind of scenes is the Avatar scene. All the avatars are just GLTFs in
     * a scene.
     */
    gameInstance.SendMessage('SceneController', 'CreateUIScene', JSON.stringify(data))
  },
  /** Sends the camera position & target to the engine */
  Teleport({ position: { x, y, z }, cameraTarget }: InstancedSpawnPoint) {
    const theY = y <= 0 ? 2 : y

    TeleportController.ensureTeleportAnimation()
    gameInstance.SendMessage('CharacterController', 'Teleport', JSON.stringify({ x, y: theY, z }))
    gameInstance.SendMessage('CameraController', 'SetRotation', JSON.stringify({ x, y: theY, z, cameraTarget }))
  },
  /** Tells the engine which scenes to load */
  LoadParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!')
    }
    gameInstance.SendMessage('SceneController', 'LoadParcelScenes', JSON.stringify(parcelsToLoad[0]))
  },
  UpdateParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!')
    }
    gameInstance.SendMessage('SceneController', 'UpdateParcelScenes', JSON.stringify(parcelsToLoad[0]))
  },
  UnloadScene(sceneId: string) {
    gameInstance.SendMessage('SceneController', 'UnloadScene', sceneId)
  },
  SendSceneMessage(messages: string) {
    gameInstance.SendMessage(`SceneController`, `SendSceneMessage`, messages)
  },
  SetSceneDebugPanel() {
    gameInstance.SendMessage('SceneController', 'SetSceneDebugPanel')
  },
  ShowFPSPanel() {
    gameInstance.SendMessage('SceneController', 'ShowFPSPanel')
  },
  HideFPSPanel() {
    gameInstance.SendMessage('SceneController', 'HideFPSPanel')
  },
  SetEngineDebugPanel() {
    gameInstance.SendMessage('SceneController', 'SetEngineDebugPanel')
  },
  ActivateRendering() {
    gameInstance.SendMessage('SceneController', 'ActivateRendering')
  },
  DeactivateRendering() {
    gameInstance.SendMessage('SceneController', 'DeactivateRendering')
  },
  UnlockCursor() {
    gameInstance.SendMessage('MouseCatcher', 'UnlockCursor')
  },
  SetBuilderReady() {
    gameInstance.SendMessage('SceneController', 'BuilderReady')
  },
  AddUserProfileToCatalog(peerProfile: ProfileForRenderer) {
    gameInstance.SendMessage('SceneController', 'AddUserProfileToCatalog', JSON.stringify(peerProfile))
  },
  AddWearablesToCatalog(wearables: Wearable[]) {
    for (const wearable of wearables) {
      gameInstance.SendMessage('SceneController', 'AddWearableToCatalog', JSON.stringify(wearable))
    }
  },
  RemoveWearablesFromCatalog(wearableIds: string[]) {
    gameInstance.SendMessage('SceneController', 'RemoveWearablesFromCatalog', JSON.stringify(wearableIds))
  },
  ClearWearableCatalog() {
    gameInstance.SendMessage('SceneController', 'ClearWearableCatalog')
  },
  ShowNewWearablesNotification(wearableNumber: number) {
    gameInstance.SendMessage('HUDController', 'ShowNewWearablesNotification', wearableNumber.toString())
  },
  ShowNotification(notification: Notification) {
    gameInstance.SendMessage('HUDController', 'ShowNotificationFromJson', JSON.stringify(notification))
  },
  ConfigureHUDElement(hudElementId: HUDElementID, configuration: HUDConfiguration) {
    gameInstance.SendMessage(
      'HUDController',
      `ConfigureHUDElement`,
      JSON.stringify({ hudElementId: hudElementId, configuration: configuration })
    )
  },
  ShowWelcomeNotification() {
    gameInstance.SendMessage('HUDController', 'ShowWelcomeNotification')
  },
  TriggerSelfUserExpression(expressionId: string) {
    gameInstance.SendMessage('HUDController', 'TriggerSelfUserExpression', expressionId)
  },
  UpdateMinimapSceneInformation(info: MinimapSceneInfo[]) {
    for (let i = 0; i < info.length; i += CHUNK_SIZE) {
      const chunk = info.slice(i, i + CHUNK_SIZE)
      gameInstance.SendMessage('SceneController', 'UpdateMinimapSceneInformation', JSON.stringify(chunk))
    }
  },
  SetTutorialEnabled() {
    if (RESET_TUTORIAL) {
      browserInterface.SaveUserTutorialStep({ tutorialStep: 0 })
    }

    gameInstance.SendMessage('TutorialController', 'SetTutorialEnabled')
  },
  SetLoadingScreenVisible(shouldShow: boolean) {
    document.getElementById('overlay')!.style.display = shouldShow ? 'block' : 'none'
    document.getElementById('load-messages-wrapper')!.style.display = shouldShow ? 'block' : 'none'
    document.getElementById('progress-bar')!.style.display = shouldShow ? 'block' : 'none'
    const loadingAudio = document.getElementById('loading-audio') as HTMLMediaElement

    if (shouldShow) {
      loadingAudio?.play().catch(e => {/*Ignored. If this fails is not critical*/})
    } else {
      loadingAudio?.pause()
    }

    if (!shouldShow && !EDITOR) {
      globalDCL.isTheFirstLoading = false
      TeleportController.stopTeleportAnimation()
    }
  },
  TriggerAirdropDisplay(data: AirdropInfo) {
    // Disabled for security reasons
  },
  AddMessageToChatWindow(message: ChatMessage) {
    gameInstance.SendMessage('SceneController', 'AddMessageToChatWindow', JSON.stringify(message))
  },
  InitializeFriends(initializationMessage: FriendsInitializationMessage) {
    gameInstance.SendMessage('SceneController', 'InitializeFriends', JSON.stringify(initializationMessage))
  },
  UpdateFriendshipStatus(updateMessage: FriendshipUpdateStatusMessage) {
    gameInstance.SendMessage('SceneController', 'UpdateFriendshipStatus', JSON.stringify(updateMessage))
  },
  UpdateUserPresence(status: UpdateUserStatusMessage) {
    gameInstance.SendMessage('SceneController', 'UpdateUserPresence', JSON.stringify(status))
  },
  FriendNotFound(queryString: string) {
    gameInstance.SendMessage('SceneController', 'FriendNotFound', JSON.stringify(queryString))
  },


  // *********************************************************************************
  // ************** Builder messages **************
  // *********************************************************************************

  // @internal
  SendBuilderMessage(method: string, payload: string = '') {
    gameInstance.SendMessage(`BuilderController`, method, payload)
  },
  SelectGizmoBuilder(type: string) {
    this.SendBuilderMessage('SelectGizmo', type)
  },
  ResetBuilderObject() {
    this.SendBuilderMessage('ResetObject')
  },
  SetCameraZoomDeltaBuilder(delta: number) {
    this.SendBuilderMessage('ZoomDelta', delta.toString())
  },
  GetCameraTargetBuilder(futureId: string) {
    this.SendBuilderMessage('GetCameraTargetBuilder', futureId)
  },
  SetPlayModeBuilder(on: string) {
    this.SendBuilderMessage('SetPlayMode', on)
  },
  PreloadFileBuilder(url: string) {
    this.SendBuilderMessage('PreloadFile', url)
  },
  GetMousePositionBuilder(x: string, y: string, id: string) {
    this.SendBuilderMessage('GetMousePosition', `{"x":"${x}", "y": "${y}", "id": "${id}" }`)
  },
  TakeScreenshotBuilder(id: string) {
    this.SendBuilderMessage('TakeScreenshot', id)
  },
  SetCameraPositionBuilder(position: ReadOnlyVector3) {
    this.SendBuilderMessage('SetBuilderCameraPosition', position.x + ',' + position.y + ',' + position.z)
  },
  SetCameraRotationBuilder(aplha: number, beta: number) {
    this.SendBuilderMessage('SetBuilderCameraRotation', aplha + ',' + beta)
  },
  ResetCameraZoomBuilder() {
    this.SendBuilderMessage('ResetBuilderCameraZoom')
  },
  SetBuilderGridResolution(position: number, rotation: number, scale: number) {
    this.SendBuilderMessage(
      'SetGridResolution',
      JSON.stringify({ position: position, rotation: rotation, scale: scale })
    )
  },
  SetBuilderSelectedEntities(entities: string[]) {
    this.SendBuilderMessage('SetSelectedEntities', JSON.stringify({ entities: entities }))
  },
  ResetBuilderScene() {
    this.SendBuilderMessage('ResetBuilderScene')
  },
  OnBuilderKeyDown(key: string) {
    this.SendBuilderMessage('OnBuilderKeyDown', key)
  }
}

globalDCL.unityInterface = unityInterface
globalDCL.rendererInterface = unityInterface
globalDCL.builderInterface = unityInterface

export type UnityInterface = typeof unityInterface

export type UnityInterfaceContainer = {
  unityInterface: UnityInterface
}

export class UnityScene<T> implements ParcelSceneAPI {
  eventDispatcher = new EventDispatcher()
  worker!: SceneWorker
  logger: ILogger

  constructor(public data: EnvironmentData<T>) {
    this.logger = createLogger(getParcelSceneID(this) + ': ')
  }

  sendBatch(actions: EntityAction[]): void {
    const sceneId = getParcelSceneID(this)
    let messages = ''
    for (let i = 0; i < actions.length; i++) {
      const action = actions[i]
      messages += this.encodeSceneMessage(sceneId, action.type, action.payload, action.tag)
      messages += '\n'
    }

    globalDCL.rendererInterface.SendSceneMessage(messages)
  }

  registerWorker(worker: SceneWorker): void {
    this.worker = worker
  }

  dispose(): void {
    // TODO: do we need to release some resource after releasing a scene worker?
  }

  on<T extends IEventNames>(event: T, cb: (event: IEvents[T]) => void): void {
    this.eventDispatcher.on(event, cb)
  }

  emit<T extends IEventNames>(event: T, data: IEvents[T]): void {
    this.eventDispatcher.emit(event, data)
  }

  encodeSceneMessage(parcelSceneId: string, method: string, payload: any, tag: string = ''): string {
    if (globalDCL.rendererInterface.debug) {
      defaultLogger.info(parcelSceneId, method, payload, tag)
    }

    let message: PB_SendSceneMessage = new PB_SendSceneMessage()
    message.setSceneid(parcelSceneId)
    message.setTag(tag)

    switch (method) {
      case 'CreateEntity':
        message.setCreateentity(this.encodeCreateEntity(payload))
        break
      case 'RemoveEntity':
        message.setRemoveentity(this.encodeRemoveEntity(payload))
        break
      case 'UpdateEntityComponent':
        message.setUpdateentitycomponent(this.encodeUpdateEntityComponent(payload))
        break
      case 'AttachEntityComponent':
        message.setAttachentitycomponent(this.encodeAttachEntityComponent(payload))
        break
      case 'ComponentRemoved':
        message.setComponentremoved(this.encodeComponentRemoved(payload))
        break
      case 'SetEntityParent':
        message.setSetentityparent(this.encodeSetEntityParent(payload))
        break
      case 'Query':
        message.setQuery(this.encodeQuery(payload))
        break
      case 'ComponentCreated':
        message.setComponentcreated(this.encodeComponentCreated(payload))
        break
      case 'ComponentDisposed':
        message.setComponentdisposed(this.encodeComponentDisposed(payload))
        break
      case 'ComponentUpdated':
        message.setComponentupdated(this.encodeComponentUpdated(payload))
        break
      case 'InitMessagesFinished':
        message.setScenestarted(new Empty()) // don't know if this is necessary
        break
      case 'OpenExternalUrl':
        message.setOpenexternalurl(this.encodeOpenExternalUrl(payload))
        break
      case 'OpenNFTDialog':
        message.setOpennftdialog(this.encodeOpenNFTDialog(payload))
        break
    }

    let arrayBuffer: Uint8Array = message.serializeBinary()
    return btoa(String.fromCharCode(...arrayBuffer))
  }

  encodeCreateEntity(createEntityPayload: CreateEntityPayload): PB_CreateEntity {
    createEntity.setId(createEntityPayload.id)
    return createEntity
  }

  encodeRemoveEntity(removeEntityPayload: RemoveEntityPayload): PB_RemoveEntity {
    removeEntity.setId(removeEntityPayload.id)
    return removeEntity
  }

  encodeUpdateEntityComponent(updateEntityComponentPayload: UpdateEntityComponentPayload): PB_UpdateEntityComponent {
    updateEntityComponent.setClassid(updateEntityComponentPayload.classId)
    updateEntityComponent.setEntityid(updateEntityComponentPayload.entityId)
    updateEntityComponent.setData(updateEntityComponentPayload.json)
    return updateEntityComponent
  }

  encodeAttachEntityComponent(attachEntityPayload: AttachEntityComponentPayload): PB_AttachEntityComponent {
    attachEntity.setEntityid(attachEntityPayload.entityId)
    attachEntity.setName(attachEntityPayload.name)
    attachEntity.setId(attachEntityPayload.id)
    return attachEntity
  }

  encodeComponentRemoved(removeEntityComponentPayload: ComponentRemovedPayload): PB_ComponentRemoved {
    removeEntityComponent.setEntityid(removeEntityComponentPayload.entityId)
    removeEntityComponent.setName(removeEntityComponentPayload.name)
    return removeEntityComponent
  }

  encodeSetEntityParent(setEntityParentPayload: SetEntityParentPayload): PB_SetEntityParent {
    setEntityParent.setEntityid(setEntityParentPayload.entityId)
    setEntityParent.setParentid(setEntityParentPayload.parentId)
    return setEntityParent
  }

  encodeQuery(queryPayload: QueryPayload): PB_Query {
    origin.setX(queryPayload.payload.ray.origin.x)
    origin.setY(queryPayload.payload.ray.origin.y)
    origin.setZ(queryPayload.payload.ray.origin.z)
    direction.setX(queryPayload.payload.ray.direction.x)
    direction.setY(queryPayload.payload.ray.direction.y)
    direction.setZ(queryPayload.payload.ray.direction.z)
    ray.setOrigin(origin)
    ray.setDirection(direction)
    ray.setDistance(queryPayload.payload.ray.distance)
    rayQuery.setRay(ray)
    rayQuery.setQueryid(queryPayload.payload.queryId)
    rayQuery.setQuerytype(queryPayload.payload.queryType)
    query.setQueryid(queryPayload.queryId)
    let arrayBuffer: Uint8Array = rayQuery.serializeBinary()
    let base64: string = btoa(String.fromCharCode(...arrayBuffer))
    query.setPayload(base64)
    return query
  }

  encodeComponentCreated(componentCreatedPayload: ComponentCreatedPayload): PB_ComponentCreated {
    componentCreated.setId(componentCreatedPayload.id)
    componentCreated.setClassid(componentCreatedPayload.classId)
    componentCreated.setName(componentCreatedPayload.name)
    return componentCreated
  }

  encodeComponentDisposed(componentDisposedPayload: ComponentDisposedPayload) {
    componentDisposed.setId(componentDisposedPayload.id)
    return componentDisposed
  }

  encodeComponentUpdated(componentUpdatedPayload: ComponentUpdatedPayload): PB_ComponentUpdated {
    componentUpdated.setId(componentUpdatedPayload.id)
    componentUpdated.setJson(componentUpdatedPayload.json)
    return componentUpdated
  }

  encodeOpenExternalUrl(url: any): PB_OpenExternalUrl {
    openExternalUrl.setUrl(url)
    return openExternalUrl
  }

  encodeOpenNFTDialog(nftDialogPayload: OpenNFTDialogPayload): PB_OpenNFTDialog {
    openNFTDialog.setAssetcontractaddress(nftDialogPayload.assetContractAddress)
    openNFTDialog.setTokenid(nftDialogPayload.tokenId)
    openNFTDialog.setComment(nftDialogPayload.comment ? nftDialogPayload.comment : '')
    return openNFTDialog
  }
}

export class UnityParcelScene extends UnityScene<LoadableParcelScene> {
  constructor(public data: EnvironmentData<LoadableParcelScene>) {
    super(data)
    this.logger = createLogger(data.data.basePosition.x + ',' + data.data.basePosition.y + ': ')
  }

  registerWorker(worker: SceneWorker): void {
    super.registerWorker(worker)

    gridToWorld(this.data.data.basePosition.x, this.data.data.basePosition.y, worker.position)

    this.worker.system
      .then(system => {
        system.getAPIInstance(DevTools).logger = this.logger

        const parcelIdentity = system.getAPIInstance(ParcelIdentity)
        parcelIdentity.land = this.data.data.land
        parcelIdentity.cid = getParcelSceneID(worker.parcelScene)
      })
      .catch(e => this.logger.error('Error initializing system', e))
  }
}

////////////////////////////////////////////////////////////////////////////////

/**
 *
 * Common initialization logic for the unity engine
 *
 * @param _gameInstance Unity game instance
 */
export async function initializeEngine(_gameInstance: GameInstance) {
  gameInstance = _gameInstance

  globalDCL.globalStore.dispatch(unityClientLoaded())
  globalDCL.rendererInterface.SetLoadingScreenVisible(true)

  globalDCL.rendererInterface.DeactivateRendering()

  if (DEBUG) {
    globalDCL.rendererInterface.SetDebug()
  }

  if (SCENE_DEBUG_PANEL) {
    globalDCL.rendererInterface.SetSceneDebugPanel()
  }

  if (SHOW_FPS_COUNTER) {
    globalDCL.rendererInterface.ShowFPSPanel()
  }

  if (ENGINE_DEBUG_PANEL) {
    globalDCL.rendererInterface.SetEngineDebugPanel()
  }

  if (tutorialEnabled()) {
    globalDCL.rendererInterface.SetTutorialEnabled()
  }

  if (!EDITOR) {
    await initializeDecentralandUI()
  }

  return {
    unityInterface,
    onMessage(type: string, message: any) {
      if (type in browserInterface) {
        // tslint:disable-next-line:semicolon
        ;(browserInterface as any)[type](message)
      } else {
        defaultLogger.info(`Unknown message (did you forget to add ${type} to unity-interface/dcl.ts?)`, message)
      }
    }
  }
}

setupPosition()

setupPointerLock()

