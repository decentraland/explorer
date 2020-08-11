import { TeleportController } from 'shared/world/TeleportController'
import { WSS_ENABLED } from 'config'
import { Vector3 } from '../decentraland-ecs/src/decentraland/math'
import { ProfileForRenderer, MinimapSceneInfo } from '../decentraland-ecs/src/decentraland/Types'
import { AirdropInfo } from 'shared/airdrops/interface'
import { Wearable } from 'shared/profiles/types'
import {
  HUDConfiguration,
  InstancedSpawnPoint,
  LoadableParcelScene,
  Notification,
  ChatMessage,
  HUDElementID,
  FriendsInitializationMessage,
  FriendshipUpdateStatusMessage,
  UpdateUserStatusMessage
} from 'shared/types'
import { nativeMsgBridge } from './nativeMessagesBridge'
import { HotSceneInfo } from 'shared/social/hotScenes'
import { defaultLogger } from 'shared/logger'

const MINIMAP_CHUNK_SIZE = 100

let _gameInstance: any = null
let originalFillMouseEventData: any

function fillMouseEventDataWrapper(eventStruct: any, e: any, target: any) {
  let widthRatio = _gameInstance.Module.canvas.widthNative / (window.innerWidth * devicePixelRatio)
  let heightRatio = _gameInstance.Module.canvas.heightNative / (window.innerHeight * devicePixelRatio)

  let eWrapper: any = {
    clientX: e.clientX * widthRatio,
    clientY: e.clientY * heightRatio,
    screenX: e.screenX,
    screenY: e.screenY,
    ctrlKey: e.ctrlKey,
    shiftKey: e.shiftKey,
    altKey: e.altKey,
    metaKey: e.metaKey,
    button: e.button,
    buttons: e.buttons,
    movementX: e['movementX'] || e['mozMovementX'] || e['webkitMovementX'],
    movementY: e['movementY'] || e['mozMovementY'] || e['webkitMovementY'],
    type: e.type
  }

  originalFillMouseEventData(eventStruct, eWrapper, target)
}

export let targetHeight: number = 1080

function resizeCanvas(module: any) {
  if (targetHeight > 2000) {
    targetHeight = window.innerHeight * devicePixelRatio
  }

  let desiredHeight = targetHeight

  let ratio = desiredHeight / module.canvas.height
  module.setCanvasSize(module.canvas.width * ratio, module.canvas.height * ratio)
}

export class UnityInterface {
  public debug: boolean = false
  public gameInstance: any
  public Module: any

  public SetTargetHeight(height: number): void {
    if (targetHeight === height) {
      return
    }

    if (!this.gameInstance.Module) {

      defaultLogger.log(
        `Can't change base resolution height to ${height}! Are you running explorer in unity editor or native?`
      )
      return
    }

    targetHeight = height
    window.dispatchEvent(new Event('resize'))
  }

  public Init(gameInstance: any): void {
    if (!WSS_ENABLED) {
      nativeMsgBridge.initNativeMessages(gameInstance)
    }

    this.gameInstance = gameInstance
    this.Module = this.gameInstance.Module
    _gameInstance = gameInstance

    if (this.Module !== undefined) {
      window.addEventListener('resize', this.resizeCanvasDelayed)
      this.resizeCanvasDelayed(null)
      this.waitForFillMouseEventData()
    }
  }

  public waitForFillMouseEventData() {
    let DCL = (window as any)['DCL']

    if (DCL.JSEvents !== undefined) {
      originalFillMouseEventData = DCL.JSEvents.fillMouseEventData
      DCL.JSEvents.fillMouseEventData = fillMouseEventDataWrapper
    } else {
      setTimeout(this.waitForFillMouseEventData, 100)
    }
  }

  public SendGenericMessage(object: string, method: string, payload: string) {
    this.gameInstance.SendMessage(object, method, payload)
  }

  public SetDebug() {
    this.gameInstance.SendMessage('SceneController', 'SetDebug')
  }

  public LoadProfile(profile: ProfileForRenderer) {
    this.gameInstance.SendMessage('SceneController', 'LoadProfile', JSON.stringify(profile))
  }

  public CreateUIScene(data: { id: string; baseUrl: string }) {
    /**
     * UI Scenes are scenes that does not check any limit or boundary. The
     * position is fixed at 0,0 and they are universe-wide. An example of this
     * kind of scenes is the Avatar scene. All the avatars are just GLTFs in
     * a scene.
     */
    this.gameInstance.SendMessage('SceneController', 'CreateUIScene', JSON.stringify(data))
  }

  /** Sends the camera position & target to the engine */

  public Teleport({ position: { x, y, z }, cameraTarget }: InstancedSpawnPoint) {
    const theY = y <= 0 ? 2 : y

    TeleportController.ensureTeleportAnimation()
    this.gameInstance.SendMessage('CharacterController', 'Teleport', JSON.stringify({ x, y: theY, z }))
    this.gameInstance.SendMessage('CameraController', 'SetRotation', JSON.stringify({ x, y: theY, z, cameraTarget }))
  }

  /** Tells the engine which scenes to load */

  public LoadParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!')
    }

    this.gameInstance.SendMessage('SceneController', 'LoadParcelScenes', JSON.stringify(parcelsToLoad[0]))
  }

  public UpdateParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!')
    }
    this.gameInstance.SendMessage('SceneController', 'UpdateParcelScenes', JSON.stringify(parcelsToLoad[0]))
  }

  public UnloadScene(sceneId: string) {
    this.gameInstance.SendMessage('SceneController', 'UnloadScene', sceneId)
  }

  public SendSceneMessage(messages: string) {
    this.gameInstance.SendMessage(`SceneController`, `SendSceneMessage`, messages)
  }

  public SetSceneDebugPanel() {
    this.gameInstance.SendMessage('SceneController', 'SetSceneDebugPanel')
  }

  public ShowFPSPanel() {
    this.gameInstance.SendMessage('SceneController', 'ShowFPSPanel')
  }

  public HideFPSPanel() {
    this.gameInstance.SendMessage('SceneController', 'HideFPSPanel')
  }

  public SetEngineDebugPanel() {
    this.gameInstance.SendMessage('SceneController', 'SetEngineDebugPanel')
  }

  public SetDisableAssetBundles() {
    this.gameInstance.SendMessage('SceneController', 'SetDisableAssetBundles')
  }

  public ActivateRendering() {
    this.gameInstance.SendMessage('SceneController', 'ActivateRendering')
  }

  public DeactivateRendering() {
    this.gameInstance.SendMessage('SceneController', 'DeactivateRendering')
  }

  public UnlockCursor() {
    this.SetCursorState(false)
  }

  public SetCursorState(locked: boolean) {
    this.gameInstance.SendMessage('MouseCatcher', 'UnlockCursorBrowser', locked ? 1 : 0)
  }

  public SetBuilderReady() {
    this.gameInstance.SendMessage('SceneController', 'BuilderReady')
  }

  public AddUserProfileToCatalog(peerProfile: ProfileForRenderer) {
    this.gameInstance.SendMessage('SceneController', 'AddUserProfileToCatalog', JSON.stringify(peerProfile))
  }

  public AddWearablesToCatalog(wearables: Wearable[]) {
    for (const wearable of wearables) {
      this.gameInstance.SendMessage('SceneController', 'AddWearableToCatalog', JSON.stringify(wearable))
    }
  }

  public RemoveWearablesFromCatalog(wearableIds: string[]) {
    this.gameInstance.SendMessage('SceneController', 'RemoveWearablesFromCatalog', JSON.stringify(wearableIds))
  }

  public ClearWearableCatalog() {
    this.gameInstance.SendMessage('SceneController', 'ClearWearableCatalog')
  }

  public ShowNewWearablesNotification(wearableNumber: number) {
    this.gameInstance.SendMessage('HUDController', 'ShowNewWearablesNotification', wearableNumber.toString())
  }

  public ShowNotification(notification: Notification) {
    this.gameInstance.SendMessage('HUDController', 'ShowNotificationFromJson', JSON.stringify(notification))
  }

  public ConfigureHUDElement(hudElementId: HUDElementID, configuration: HUDConfiguration) {
    this.gameInstance.SendMessage(
      'HUDController',
      `ConfigureHUDElement`,
      JSON.stringify({ hudElementId: hudElementId, configuration: configuration })
    )
  }

  public ShowWelcomeNotification() {
    this.gameInstance.SendMessage('HUDController', 'ShowWelcomeNotification')
  }

  public TriggerSelfUserExpression(expressionId: string) {
    this.gameInstance.SendMessage('HUDController', 'TriggerSelfUserExpression', expressionId)
  }

  public UpdateMinimapSceneInformation(info: MinimapSceneInfo[]) {
    for (let i = 0; i < info.length; i += MINIMAP_CHUNK_SIZE) {
      const chunk = info.slice(i, i + MINIMAP_CHUNK_SIZE)
      this.gameInstance.SendMessage('SceneController', 'UpdateMinimapSceneInformation', JSON.stringify(chunk))
    }
  }

  public SetTutorialEnabled() {
    this.gameInstance.SendMessage('TutorialController', 'SetTutorialEnabled')
  }

  public TriggerAirdropDisplay(data: AirdropInfo) {
    // Disabled for security reasons
  }

  public AddMessageToChatWindow(message: ChatMessage) {
    this.gameInstance.SendMessage('SceneController', 'AddMessageToChatWindow', JSON.stringify(message))
  }

  public InitializeFriends(initializationMessage: FriendsInitializationMessage) {
    this.gameInstance.SendMessage('SceneController', 'InitializeFriends', JSON.stringify(initializationMessage))
  }

  public UpdateFriendshipStatus(updateMessage: FriendshipUpdateStatusMessage) {
    this.gameInstance.SendMessage('SceneController', 'UpdateFriendshipStatus', JSON.stringify(updateMessage))
  }

  public UpdateUserPresence(status: UpdateUserStatusMessage) {
    this.gameInstance.SendMessage('SceneController', 'UpdateUserPresence', JSON.stringify(status))
  }

  public FriendNotFound(queryString: string) {
    this.gameInstance.SendMessage('SceneController', 'FriendNotFound', JSON.stringify(queryString))
  }

  public RequestTeleport(teleportData: {}) {
    this.gameInstance.SendMessage('HUDController', 'RequestTeleport', JSON.stringify(teleportData))
  }

  public UpdateHotScenesList(info: HotSceneInfo[]) {
    const chunks = []

    while (info.length) {
      chunks.push(info.splice(0, MINIMAP_CHUNK_SIZE))
    }

    for (let i = 0; i < chunks.length; i++) {
      const payload = { chunkIndex: i, chunksCount: chunks.length, scenesInfo: chunks[i] }
      this.gameInstance.SendMessage('SceneController', 'UpdateHotScenesList', JSON.stringify(payload))
    }
  }

  // *********************************************************************************
  // ************** Builder messages **************
  // *********************************************************************************
  // @internal

  public SendBuilderMessage(method: string, payload: string = '') {
    this.gameInstance.SendMessage(`BuilderController`, method, payload)
  }

  public SelectGizmoBuilder(type: string) {
    this.SendBuilderMessage('SelectGizmo', type)
  }

  public ResetBuilderObject() {
    this.SendBuilderMessage('ResetObject')
  }

  public SetCameraZoomDeltaBuilder(delta: number) {
    this.SendBuilderMessage('ZoomDelta', delta.toString())
  }

  public GetCameraTargetBuilder(futureId: string) {
    this.SendBuilderMessage('GetCameraTargetBuilder', futureId)
  }

  public SetPlayModeBuilder(on: string) {
    this.SendBuilderMessage('SetPlayMode', on)
  }

  public PreloadFileBuilder(url: string) {
    this.SendBuilderMessage('PreloadFile', url)
  }

  public GetMousePositionBuilder(x: string, y: string, id: string) {
    this.SendBuilderMessage('GetMousePosition', `{"x":"${x}", "y": "${y}", "id": "${id}" }`)
  }

  public TakeScreenshotBuilder(id: string) {
    this.SendBuilderMessage('TakeScreenshot', id)
  }

  public SetCameraPositionBuilder(position: Vector3) {
    this.SendBuilderMessage('SetBuilderCameraPosition', position.x + ',' + position.y + ',' + position.z)
  }

  public SetCameraRotationBuilder(aplha: number, beta: number) {
    this.SendBuilderMessage('SetBuilderCameraRotation', aplha + ',' + beta)
  }

  public ResetCameraZoomBuilder() {
    this.SendBuilderMessage('ResetBuilderCameraZoom')
  }

  public SetBuilderGridResolution(position: number, rotation: number, scale: number) {
    this.SendBuilderMessage(
      'SetGridResolution',
      JSON.stringify({ position: position, rotation: rotation, scale: scale })
    )
  }

  public SetBuilderSelectedEntities(entities: string[]) {
    this.SendBuilderMessage('SetSelectedEntities', JSON.stringify({ entities: entities }))
  }

  public ResetBuilderScene() {
    this.SendBuilderMessage('ResetBuilderScene')
  }

  public OnBuilderKeyDown(key: string) {
    this.SendBuilderMessage('OnBuilderKeyDown', key)
  }

  private resizeCanvasDelayed(ev: UIEvent | null) {
    window.setTimeout(() => {
      resizeCanvas(_gameInstance.Module)
    }, 100)
  }
}

export let unityInterface: UnityInterface = new UnityInterface()
