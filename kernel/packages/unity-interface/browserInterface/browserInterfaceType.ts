import { ReadOnlyQuaternion, ReadOnlyVector3 } from 'decentraland-ecs/src/decentraland/math'
import { Avatar } from 'shared/profiles/types'
import { ChatMessage, FriendshipUpdateStatusMessage, WorldPosition } from 'shared/types'

export type browserInterfaceType = {
  /** Triggered when the camera moves */
  ReportPosition: (data: { position: ReadOnlyVector3; rotation: ReadOnlyQuaternion; playerHeight?: number }) => void
  ReportMousePosition: (data: { id: string; mousePosition: ReadOnlyVector3 }) => void
  SceneEvent: (data: { sceneId: string; eventType: string; payload: any }) => void
  OpenWebURL: (data: { url: string }) => void
  PerformanceReport: (samples: string) => void
  PreloadFinished: (data: { sceneId: string }) => void
  TriggerExpression: (data: { id: string; timestamp: number }) => void
  TermsOfServiceResponse: (sceneId: string, accepted: boolean, dontShowAgain: boolean) => void
  MotdConfirmClicked: () => void
  GoTo: (data: { x: number; y: number }) => void
  LogOut: () => void
  SaveUserAvatar: (changes: { face: string; body: string; avatar: Avatar }) => void
  SaveUserTutorialStep: (data: { tutorialStep: number }) => void
  ControlEvent: ({ eventType, payload }: { eventType: string; payload: any }) => void
  SendScreenshot: (data: { id: string; encodedTexture: string }) => void
  ReportBuilderCameraTarget: (data: { id: string; cameraTarget: ReadOnlyVector3 }) => void
  UserAcceptedCollectibles: (data: { id: string }) => void
  EditAvatarClicked: () => void
  ReportScene: (sceneId: string) => void
  ReportPlayer: (username: string) => void
  BlockPlayer: (data: { userId: string }) => void
  UnblockPlayer: (data: { userId: string }) => void
  ReportUserEmail: (data: { userEmail: string }) => void
  RequestScenesInfoInArea: (data: {
    parcel: {
      x: number
      y: number
    }
    scenesAround: number
  }) => void
  SetAudioStream: (data: { url: string; play: boolean; volume: number }) => void
  SendChatMessage: (data: { message: ChatMessage }) => void
  UpdateFriendshipStatus: (message: FriendshipUpdateStatusMessage) => Promise<void>
  JumpIn: (data: WorldPosition) => Promise<void>
}
