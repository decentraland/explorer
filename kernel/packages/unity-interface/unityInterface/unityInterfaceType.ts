import { MinimapSceneInfo, ProfileForRenderer } from 'decentraland-ecs/src/decentraland/Types'
import { Wearable } from 'shared/profiles/types'
import {
  ChatMessage,
  FriendshipUpdateStatusMessage,
  FriendsInitializationMessage,
  HUDConfiguration,
  HUDElementID,
  InstancedSpawnPoint,
  LoadableParcelScene,
  Notification,
  UpdateUserStatusMessage
} from 'shared/types'
import { AirdropInfo } from 'shared/airdrops/interface'

export type unityInterfaceType = {
  debug: boolean
  SendGenericMessage: (object: string, method: string, payload: string) => void
  SetDebug: () => void
  LoadProfile: (profile: ProfileForRenderer) => void
  CreateUIScene: (data: { id: string; baseUrl: string }) => void
  /** Sends the camera position & target to the engine */
  Teleport: ({ position: { x, y, z }, cameraTarget }: InstancedSpawnPoint) => void
  /** Tells the engine which scenes to load */
  LoadParcelScenes: (parcelsToLoad: LoadableParcelScene[]) => void
  UpdateParcelScenes: (parcelsToLoad: LoadableParcelScene[]) => void
  UnloadScene: (sceneId: string) => void
  SendSceneMessage: (messages: string) => void
  SetSceneDebugPanel: () => void
  ShowFPSPanel: () => void
  HideFPSPanel: () => void
  SetEngineDebugPanel: () => void
  // @internal
  SendBuilderMessage: (method: string, payload?: string) => void
  ActivateRendering: () => void
  DeactivateRendering: () => void
  UnlockCursor: () => void
  SetBuilderReady: () => void
  AddUserProfileToCatalog: (peerProfile: ProfileForRenderer) => void
  AddWearablesToCatalog: (wearables: Wearable[]) => void
  RemoveWearablesFromCatalog: (wearableIds: string[]) => void
  ClearWearableCatalog: () => void
  ShowNewWearablesNotification: (wearableNumber: number) => void
  ShowNotification: (notification: Notification) => void
  ConfigureHUDElement: (hudElementId: HUDElementID, configuration: HUDConfiguration) => void
  ShowWelcomeNotification: () => void
  TriggerSelfUserExpression: (expressionId: string) => void
  UpdateMinimapSceneInformation: (info: MinimapSceneInfo[]) => void
  SetTutorialEnabled: () => void
  TriggerAirdropDisplay: (data: AirdropInfo) => void
  AddMessageToChatWindow: (message: ChatMessage) => void
  InitializeFriends: (initializationMessage: FriendsInitializationMessage) => void
  UpdateFriendshipStatus: (updateMessage: FriendshipUpdateStatusMessage) => void
  UpdateUserPresence: (updateMessage: UpdateUserStatusMessage) => void
  FriendNotFound: (queryString: string) => void
}
