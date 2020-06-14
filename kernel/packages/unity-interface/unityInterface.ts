import {
  EDITOR,

  RESET_TUTORIAL
} from 'config';
import { ReadOnlyVector3 } from 'decentraland-ecs/src/decentraland/math';
import { MinimapSceneInfo, ProfileForRenderer } from 'decentraland-ecs/src/decentraland/Types';
import { AirdropInfo } from 'shared/airdrops/interface';
import { globalDCL } from 'shared/globalDCL';
import { Wearable } from 'shared/profiles/types';
import { builderInterfaceType } from 'shared/renderer-interface/builder/builderInterface';
import { rendererInterfaceType } from 'shared/renderer-interface/rendererInterface/rendererInterfaceType';
import { ChatMessage, FriendshipUpdateStatusMessage, FriendsInitializationMessage, HUDConfiguration, HUDElementID, InstancedSpawnPoint, LoadableParcelScene, Notification, UpdateUserStatusMessage } from 'shared/types';
import { TeleportController } from 'shared/world/TeleportController';
import { browserInterface } from './browserInterface';
import { CHUNK_SIZE } from './dcl';

export const unityInterface: rendererInterfaceType & builderInterfaceType = {
  debug: false,

  SendGenericMessage(object: string, method: string, payload: string) {
    globalDCL.lowLevelInterface.SendMessage(object, method, payload);
  },
  SetDebug() {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'SetDebug');
  },
  LoadProfile(profile: ProfileForRenderer) {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'LoadProfile', JSON.stringify(profile));
  },
  CreateUIScene(data: { id: string; baseUrl: string; }) {
    /**
     * UI Scenes are scenes that does not check any limit or boundary. The
     * position is fixed at 0,0 and they are universe-wide. An example of this
     * kind of scenes is the Avatar scene. All the avatars are just GLTFs in
     * a scene.
     */
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'CreateUIScene', JSON.stringify(data));
  },
  /** Sends the camera position & target to the engine */
  Teleport({ position: { x, y, z }, cameraTarget }: InstancedSpawnPoint) {
    const theY = y <= 0 ? 2 : y;

    TeleportController.ensureTeleportAnimation();
    globalDCL.lowLevelInterface.SendMessage('CharacterController', 'Teleport', JSON.stringify({ x, y: theY, z }));
    globalDCL.lowLevelInterface.SendMessage('CameraController', 'SetRotation', JSON.stringify({ x, y: theY, z, cameraTarget }));
  },
  /** Tells the engine which scenes to load */
  LoadParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!');
    }
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'LoadParcelScenes', JSON.stringify(parcelsToLoad[0]));
  },
  UpdateParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!');
    }
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'UpdateParcelScenes', JSON.stringify(parcelsToLoad[0]));
  },
  UnloadScene(sceneId: string) {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'UnloadScene', sceneId);
  },
  SendSceneMessage(messages: string) {
    globalDCL.lowLevelInterface.SendMessage(`SceneController`, `SendSceneMessage`, messages);
  },
  SetSceneDebugPanel() {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'SetSceneDebugPanel');
  },
  ShowFPSPanel() {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'ShowFPSPanel');
  },
  HideFPSPanel() {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'HideFPSPanel');
  },
  SetEngineDebugPanel() {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'SetEngineDebugPanel');
  },
  ActivateRendering() {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'ActivateRendering');
  },
  DeactivateRendering() {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'DeactivateRendering');
  },
  UnlockCursor() {
    globalDCL.lowLevelInterface.SendMessage('MouseCatcher', 'UnlockCursor');
  },
  SetBuilderReady() {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'BuilderReady');
  },
  AddUserProfileToCatalog(peerProfile: ProfileForRenderer) {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'AddUserProfileToCatalog', JSON.stringify(peerProfile));
  },
  AddWearablesToCatalog(wearables: Wearable[]) {
    for (const wearable of wearables) {
      globalDCL.lowLevelInterface.SendMessage('SceneController', 'AddWearableToCatalog', JSON.stringify(wearable));
    }
  },
  RemoveWearablesFromCatalog(wearableIds: string[]) {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'RemoveWearablesFromCatalog', JSON.stringify(wearableIds));
  },
  ClearWearableCatalog() {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'ClearWearableCatalog');
  },
  ShowNewWearablesNotification(wearableNumber: number) {
    globalDCL.lowLevelInterface.SendMessage('HUDController', 'ShowNewWearablesNotification', wearableNumber.toString());
  },
  ShowNotification(notification: Notification) {
    globalDCL.lowLevelInterface.SendMessage('HUDController', 'ShowNotificationFromJson', JSON.stringify(notification));
  },
  ConfigureHUDElement(hudElementId: HUDElementID, configuration: HUDConfiguration) {
    globalDCL.lowLevelInterface.SendMessage(
      'HUDController',
      `ConfigureHUDElement`,
      JSON.stringify({ hudElementId: hudElementId, configuration: configuration })
    );
  },
  ShowWelcomeNotification() {
    globalDCL.lowLevelInterface.SendMessage('HUDController', 'ShowWelcomeNotification');
  },
  TriggerSelfUserExpression(expressionId: string) {
    globalDCL.lowLevelInterface.SendMessage('HUDController', 'TriggerSelfUserExpression', expressionId);
  },
  UpdateMinimapSceneInformation(info: MinimapSceneInfo[]) {
    for (let i = 0; i < info.length; i += CHUNK_SIZE) {
      const chunk = info.slice(i, i + CHUNK_SIZE);
      globalDCL.lowLevelInterface.SendMessage('SceneController', 'UpdateMinimapSceneInformation', JSON.stringify(chunk));
    }
  },
  SetTutorialEnabled() {
    if (RESET_TUTORIAL) {
      browserInterface.SaveUserTutorialStep({ tutorialStep: 0 });
    }

    globalDCL.lowLevelInterface.SendMessage('TutorialController', 'SetTutorialEnabled');
  },
  SetLoadingScreenVisible(shouldShow: boolean) {
    document.getElementById('overlay')!.style.display = shouldShow ? 'block' : 'none';
    document.getElementById('load-messages-wrapper')!.style.display = shouldShow ? 'block' : 'none';
    document.getElementById('progress-bar')!.style.display = shouldShow ? 'block' : 'none';
    const loadingAudio = document.getElementById('loading-audio') as HTMLMediaElement;

    if (shouldShow) {
      loadingAudio?.play().catch(e => { });
    }
    else {
      loadingAudio?.pause();
    }

    if (!shouldShow && !EDITOR) {
      globalDCL.isTheFirstLoading = false;
      TeleportController.stopTeleportAnimation();
    }
  },
  TriggerAirdropDisplay(data: AirdropInfo) {
    // Disabled for security reasons
  },
  AddMessageToChatWindow(message: ChatMessage) {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'AddMessageToChatWindow', JSON.stringify(message));
  },
  InitializeFriends(initializationMessage: FriendsInitializationMessage) {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'InitializeFriends', JSON.stringify(initializationMessage));
  },
  UpdateFriendshipStatus(updateMessage: FriendshipUpdateStatusMessage) {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'UpdateFriendshipStatus', JSON.stringify(updateMessage));
  },
  UpdateUserPresence(status: UpdateUserStatusMessage) {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'UpdateUserPresence', JSON.stringify(status));
  },
  FriendNotFound(queryString: string) {
    globalDCL.lowLevelInterface.SendMessage('SceneController', 'FriendNotFound', JSON.stringify(queryString));
  },
  RequestTeleport(teleportData: {}) {
    gameInstance.SendMessage('HUDController', 'RequestTeleport', JSON.stringify(teleportData))
  },

  // *********************************************************************************
  // ************** Builder messages **************
  // *********************************************************************************
  // @internal
  SendBuilderMessage(method: string, payload: string = '') {
    globalDCL.lowLevelInterface.SendMessage(`BuilderController`, method, payload);
  },
  SelectGizmoBuilder(type: string) {
    this.SendBuilderMessage('SelectGizmo', type);
  },
  ResetBuilderObject() {
    this.SendBuilderMessage('ResetObject');
  },
  SetCameraZoomDeltaBuilder(delta: number) {
    this.SendBuilderMessage('ZoomDelta', delta.toString());
  },
  GetCameraTargetBuilder(futureId: string) {
    this.SendBuilderMessage('GetCameraTargetBuilder', futureId);
  },
  SetPlayModeBuilder(on: string) {
    this.SendBuilderMessage('SetPlayMode', on);
  },
  PreloadFileBuilder(url: string) {
    this.SendBuilderMessage('PreloadFile', url);
  },
  GetMousePositionBuilder(x: string, y: string, id: string) {
    this.SendBuilderMessage('GetMousePosition', `{"x":"${x}", "y": "${y}", "id": "${id}" }`);
  },
  TakeScreenshotBuilder(id: string) {
    this.SendBuilderMessage('TakeScreenshot', id);
  },
  SetCameraPositionBuilder(position: ReadOnlyVector3) {
    this.SendBuilderMessage('SetBuilderCameraPosition', position.x + ',' + position.y + ',' + position.z);
  },
  SetCameraRotationBuilder(aplha: number, beta: number) {
    this.SendBuilderMessage('SetBuilderCameraRotation', aplha + ',' + beta);
  },
  ResetCameraZoomBuilder() {
    this.SendBuilderMessage('ResetBuilderCameraZoom');
  },
  SetBuilderGridResolution(position: number, rotation: number, scale: number) {
    this.SendBuilderMessage(
      'SetGridResolution',
      JSON.stringify({ position: position, rotation: rotation, scale: scale })
    );
  },
  SetBuilderSelectedEntities(entities: string[]) {
    this.SendBuilderMessage('SetSelectedEntities', JSON.stringify({ entities: entities }));
  },
  ResetBuilderScene() {
    this.SendBuilderMessage('ResetBuilderScene');
  },
  OnBuilderKeyDown(key: string) {
    this.SendBuilderMessage('OnBuilderKeyDown', key);
  }
};
