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
import { gameInstance, CHUNK_SIZE } from './dcl';

export const unityInterface: rendererInterfaceType & builderInterfaceType = {
  debug: false,

  SendGenericMessage(object: string, method: string, payload: string) {
    gameInstance.SendMessage(object, method, payload);
  },
  SetDebug() {
    gameInstance.SendMessage('SceneController', 'SetDebug');
  },
  LoadProfile(profile: ProfileForRenderer) {
    gameInstance.SendMessage('SceneController', 'LoadProfile', JSON.stringify(profile));
  },
  CreateUIScene(data: { id: string; baseUrl: string; }) {
    /**
     * UI Scenes are scenes that does not check any limit or boundary. The
     * position is fixed at 0,0 and they are universe-wide. An example of this
     * kind of scenes is the Avatar scene. All the avatars are just GLTFs in
     * a scene.
     */
    gameInstance.SendMessage('SceneController', 'CreateUIScene', JSON.stringify(data));
  },
  /** Sends the camera position & target to the engine */
  Teleport({ position: { x, y, z }, cameraTarget }: InstancedSpawnPoint) {
    const theY = y <= 0 ? 2 : y;

    TeleportController.ensureTeleportAnimation();
    gameInstance.SendMessage('CharacterController', 'Teleport', JSON.stringify({ x, y: theY, z }));
    gameInstance.SendMessage('CameraController', 'SetRotation', JSON.stringify({ x, y: theY, z, cameraTarget }));
  },
  /** Tells the engine which scenes to load */
  LoadParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!');
    }
    gameInstance.SendMessage('SceneController', 'LoadParcelScenes', JSON.stringify(parcelsToLoad[0]));
  },
  UpdateParcelScenes(parcelsToLoad: LoadableParcelScene[]) {
    if (parcelsToLoad.length > 1) {
      throw new Error('Only one scene at a time!');
    }
    gameInstance.SendMessage('SceneController', 'UpdateParcelScenes', JSON.stringify(parcelsToLoad[0]));
  },
  UnloadScene(sceneId: string) {
    gameInstance.SendMessage('SceneController', 'UnloadScene', sceneId);
  },
  SendSceneMessage(messages: string) {
    gameInstance.SendMessage(`SceneController`, `SendSceneMessage`, messages);
  },
  SetSceneDebugPanel() {
    gameInstance.SendMessage('SceneController', 'SetSceneDebugPanel');
  },
  ShowFPSPanel() {
    gameInstance.SendMessage('SceneController', 'ShowFPSPanel');
  },
  HideFPSPanel() {
    gameInstance.SendMessage('SceneController', 'HideFPSPanel');
  },
  SetEngineDebugPanel() {
    gameInstance.SendMessage('SceneController', 'SetEngineDebugPanel');
  },
  ActivateRendering() {
    gameInstance.SendMessage('SceneController', 'ActivateRendering');
  },
  DeactivateRendering() {
    gameInstance.SendMessage('SceneController', 'DeactivateRendering');
  },
  UnlockCursor() {
    gameInstance.SendMessage('MouseCatcher', 'UnlockCursor');
  },
  SetBuilderReady() {
    gameInstance.SendMessage('SceneController', 'BuilderReady');
  },
  AddUserProfileToCatalog(peerProfile: ProfileForRenderer) {
    gameInstance.SendMessage('SceneController', 'AddUserProfileToCatalog', JSON.stringify(peerProfile));
  },
  AddWearablesToCatalog(wearables: Wearable[]) {
    for (const wearable of wearables) {
      gameInstance.SendMessage('SceneController', 'AddWearableToCatalog', JSON.stringify(wearable));
    }
  },
  RemoveWearablesFromCatalog(wearableIds: string[]) {
    gameInstance.SendMessage('SceneController', 'RemoveWearablesFromCatalog', JSON.stringify(wearableIds));
  },
  ClearWearableCatalog() {
    gameInstance.SendMessage('SceneController', 'ClearWearableCatalog');
  },
  ShowNewWearablesNotification(wearableNumber: number) {
    gameInstance.SendMessage('HUDController', 'ShowNewWearablesNotification', wearableNumber.toString());
  },
  ShowNotification(notification: Notification) {
    gameInstance.SendMessage('HUDController', 'ShowNotificationFromJson', JSON.stringify(notification));
  },
  ConfigureHUDElement(hudElementId: HUDElementID, configuration: HUDConfiguration) {
    gameInstance.SendMessage(
      'HUDController',
      `ConfigureHUDElement`,
      JSON.stringify({ hudElementId: hudElementId, configuration: configuration })
    );
  },
  ShowWelcomeNotification() {
    gameInstance.SendMessage('HUDController', 'ShowWelcomeNotification');
  },
  TriggerSelfUserExpression(expressionId: string) {
    gameInstance.SendMessage('HUDController', 'TriggerSelfUserExpression', expressionId);
  },
  UpdateMinimapSceneInformation(info: MinimapSceneInfo[]) {
    for (let i = 0; i < info.length; i += CHUNK_SIZE) {
      const chunk = info.slice(i, i + CHUNK_SIZE);
      gameInstance.SendMessage('SceneController', 'UpdateMinimapSceneInformation', JSON.stringify(chunk));
    }
  },
  SetTutorialEnabled() {
    if (RESET_TUTORIAL) {
      browserInterface.SaveUserTutorialStep({ tutorialStep: 0 });
    }

    gameInstance.SendMessage('TutorialController', 'SetTutorialEnabled');
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
    gameInstance.SendMessage('SceneController', 'AddMessageToChatWindow', JSON.stringify(message));
  },
  InitializeFriends(initializationMessage: FriendsInitializationMessage) {
    gameInstance.SendMessage('SceneController', 'InitializeFriends', JSON.stringify(initializationMessage));
  },
  UpdateFriendshipStatus(updateMessage: FriendshipUpdateStatusMessage) {
    gameInstance.SendMessage('SceneController', 'UpdateFriendshipStatus', JSON.stringify(updateMessage));
  },
  UpdateUserPresence(status: UpdateUserStatusMessage) {
    gameInstance.SendMessage('SceneController', 'UpdateUserPresence', JSON.stringify(status));
  },
  FriendNotFound(queryString: string) {
    gameInstance.SendMessage('SceneController', 'FriendNotFound', JSON.stringify(queryString));
  },
  RequestTeleport(teleportData: {}) {
    gameInstance.SendMessage('HUDController', 'RequestTeleport', JSON.stringify(teleportData))
  },

  // *********************************************************************************
  // ************** Builder messages **************
  // *********************************************************************************
  // @internal
  SendBuilderMessage(method: string, payload: string = '') {
    gameInstance.SendMessage(`BuilderController`, method, payload);
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
