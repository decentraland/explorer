import { uuid } from 'decentraland-ecs/src'
import { IFuture } from 'fp-future'
import { identity } from 'shared'
import { persistCurrentUser, sendPublicChatMessage } from 'shared/comms'
import { AvatarMessageType } from 'shared/comms/interface/types'
import { avatarMessageObservable, getUserProfile } from 'shared/comms/peers'
import { providerFuture } from 'shared/ethereum/provider'
import { getProfile, hasConnectedWeb3 } from 'shared/profiles/selectors'
import { TeleportController } from 'shared/world/TeleportController'
import { reportScenesAroundParcel } from 'shared/atlas/actions'
import {
  DEBUG,
  EDITOR,
  ENGINE_DEBUG_PANEL,
  playerConfigurations,
  SCENE_DEBUG_PANEL,
  SHOW_FPS_COUNTER,
  ethereumConfigurations,
  NO_ASSET_BUNDLES,
  WSS_ENABLED} from 'config'
import { Quaternion, ReadOnlyQuaternion, ReadOnlyVector3, Vector3 } from '../decentraland-ecs/src/decentraland/math'
import { IEventNames, ProfileForRenderer, MinimapSceneInfo } from '../decentraland-ecs/src/decentraland/Types'
import { sceneLifeCycleObservable } from '../decentraland-loader/lifecycle/controllers/scene'
import { AirdropInfo } from 'shared/airdrops/interface'
import { queueTrackingEvent } from 'shared/analytics'
import { aborted } from 'shared/loading/ReportFatalError'
import { loadingScenes, teleportTriggered, unityClientLoaded } from 'shared/loading/types'
import { defaultLogger } from 'shared/logger'
import { saveProfileRequest } from 'shared/profiles/actions'
import { Avatar, Profile, Wearable } from 'shared/profiles/types'
import { Session } from 'shared/session'
import { getPerformanceInfo } from 'shared/session/getPerformanceInfo'
import {
  HUDConfiguration,
  ILand,
  InstancedSpawnPoint,
  SceneJsonData,
  LoadableParcelScene,
  MappingsResponse,
  Notification,
  ChatMessage,
  HUDElementID,
  FriendsInitializationMessage,
  FriendshipUpdateStatusMessage,
  UpdateUserStatusMessage,
  FriendshipAction,
  WorldPosition,
} from 'shared/types'
import {
  enableParcelSceneLoading,
  getParcelSceneID,
  getSceneWorkerBySceneID,
  loadParcelScene,
  stopParcelSceneWorker
} from 'shared/world/parcelSceneManager'
import { positionObservable, teleportObservable } from 'shared/world/positionThings'
import { hudWorkerUrl, SceneWorker } from 'shared/world/SceneWorker'
import { ensureUiApis } from 'shared/world/uiSceneInitializer'
import { worldRunningObservable } from 'shared/world/worldState'
import { profileToRendererFormat } from 'shared/profiles/transformations/profileToRendererFormat'
import { StoreContainer } from 'shared/store/rootTypes'
import { ILandToLoadableParcelScene, ILandToLoadableParcelSceneUpdate } from 'shared/selectors'
import { sendMessage, updateUserData, updateFriendship } from 'shared/chat/actions'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'
import { changeRealm, catalystRealmConnected, candidatesFetched } from 'shared/dao'
import { notifyStatusThroughChat } from 'shared/comms/chat'
import { getAppNetwork, fetchOwner } from 'shared/web3'
import { updateStatusMessage } from 'shared/loading/actions'
import { NativeMessagesBridge } from './nativeMessagesBridge'
import { ProtobufMessagesBridge } from './protobufMessagesBridge'
import { UnityScene } from './UnityScene'
import { UnityParcelScene } from './UnityParcelScene'

declare const globalThis: UnityInterfaceContainer &
  BrowserInterfaceContainer &
  StoreContainer & { analytics: any; delighted: any }

type GameInstance = {
  SendMessage(object: string, method: string, ...args: (number | string)[]): void
}

const rendererVersion = require('decentraland-renderer')
window['console'].log('Renderer version: ' + rendererVersion)

let gameInstance!: GameInstance
let isTheFirstLoading = true

export let futures: Record<string, IFuture<any>> = {}
export let hasWallet: boolean = false

const positionEvent = {
  position: Vector3.Zero(),
  quaternion: Quaternion.Identity,
  rotation: Vector3.Zero(),
  playerHeight: playerConfigurations.height,
  mousePosition: Vector3.Zero()
}

/////////////////////////////////// AUDIO STREAMING ///////////////////////////////////

const audioStreamSource = new Audio()

teleportObservable.add(() => {
  audioStreamSource.pause()
})

async function setAudioStream(url: string, play: boolean, volume: number) {
  const isSameSrc = audioStreamSource.src.length > 1 && url.includes(audioStreamSource.src)
  const playSrc = play && (!isSameSrc || (isSameSrc && audioStreamSource.paused))

  audioStreamSource.volume = volume

  if (play && !isSameSrc) {
    audioStreamSource.src = url
  } else if (!play && isSameSrc) {
    audioStreamSource.pause()
  }

  if (playSrc) {
    try {
      await audioStreamSource.play()
    } catch (err) {
      defaultLogger.log('setAudioStream: failed to play' + err)
    }
  }
}

/////////////////////////////////// HANDLERS ///////////////////////////////////

const browserInterface = {
  /** Triggered when the camera moves */
  ReportPosition(data: { position: ReadOnlyVector3; rotation: ReadOnlyQuaternion; playerHeight?: number }) {
    positionEvent.position.set(data.position.x, data.position.y, data.position.z)
    positionEvent.quaternion.set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w)
    positionEvent.rotation.copyFrom(positionEvent.quaternion.eulerAngles)
    positionEvent.playerHeight = data.playerHeight || playerConfigurations.height
    positionObservable.notifyObservers(positionEvent)
  },

  ReportMousePosition(data: { id: string; mousePosition: ReadOnlyVector3 }) {
    positionEvent.mousePosition.set(data.mousePosition.x, data.mousePosition.y, data.mousePosition.z)
    positionObservable.notifyObservers(positionEvent)
    futures[data.id].resolve(data.mousePosition)
  },

  SceneEvent(data: { sceneId: string; eventType: string; payload: any }) {
    const scene = getSceneWorkerBySceneID(data.sceneId)
    if (scene) {
      const parcelScene = scene.parcelScene as UnityParcelScene
      parcelScene.emit(data.eventType as IEventNames, data.payload)
    } else {
      if (data.eventType !== 'metricsUpdate') {
        defaultLogger.error(`SceneEvent: Scene ${data.sceneId} not found`, data)
      }
    }
  },

  OpenWebURL(data: { url: string }) {
    const newWindow: any = window.open(data.url, '_blank', 'noopener,noreferrer')
    if (newWindow != null) newWindow.opener = null
  },

  PerformanceHiccupReport(data: { hiccupsInThousandFrames: number; hiccupsTime: number; totalTime: number }) {
    queueTrackingEvent('hiccup report', data)
  },

  PerformanceReport(samples: string) {
    const perfReport = getPerformanceInfo(samples)
    queueTrackingEvent('performance report', perfReport)
  },

  PreloadFinished(data: { sceneId: string }) {
    // stub. there is no code about this in unity side yet
  },

  TriggerExpression(data: { id: string; timestamp: number }) {
    avatarMessageObservable.notifyObservers({
      type: AvatarMessageType.USER_EXPRESSION,
      uuid: uuid(),
      expressionId: data.id,
      timestamp: data.timestamp
    })
    const messageId = uuid()
    const body = `␐${data.id} ${data.timestamp}`

    sendPublicChatMessage(messageId, body)
  },

  TermsOfServiceResponse(sceneId: string, accepted: boolean, dontShowAgain: boolean) {
    // TODO
  },

  MotdConfirmClicked() {
    if (hasWallet) {
      TeleportController.goToNext()
    } else {
      window.open('https://docs.decentraland.org/get-a-wallet/', '_blank')
    }
  },

  GoTo(data: { x: number; y: number }) {
    TeleportController.goTo(data.x, data.y)
  },

  GoToMagic() {
    TeleportController.goToMagic()
  },

  GoToCrowd() {
    TeleportController.goToCrowd().catch((e) => defaultLogger.error('error goToCrowd', e))
  },

  LogOut() {
    Session.current.then((s) => s.logout()).catch((e) => defaultLogger.error('error while logging out', e))
  },

  SaveUserAvatar(changes: { face: string; face128: string; face256: string; body: string; avatar: Avatar }) {
    const { face, face128, face256, body, avatar } = changes
    const profile: Profile = getUserProfile().profile as Profile
    const updated = { ...profile, avatar: { ...avatar, snapshots: { face, face128, face256, body } } }
    globalThis.globalStore.dispatch(saveProfileRequest(updated))
  },

  SaveUserTutorialStep(data: { tutorialStep: number }) {
    const profile: Profile = getUserProfile().profile as Profile
    profile.tutorialStep = data.tutorialStep
    globalThis.globalStore.dispatch(saveProfileRequest(profile))

    persistCurrentUser({
      version: profile.version,
      profile: profileToRendererFormat(profile, identity)
    })
  },

  ControlEvent({ eventType, payload }: { eventType: string; payload: any }) {
    switch (eventType) {
      case 'SceneReady': {
        const { sceneId } = payload
        sceneLifeCycleObservable.notifyObservers({ sceneId, status: 'ready' })
        break
      }
      case 'ActivateRenderingACK': {
        if (!aborted) {
          worldRunningObservable.notifyObservers(true)
        }
        break
      }
      default: {
        defaultLogger.warn(`Unknown event type ${eventType}, ignoring`)
        break
      }
    }
  },

  SendScreenshot(data: { id: string; encodedTexture: string }) {
    futures[data.id].resolve(data.encodedTexture)
  },

  ReportBuilderCameraTarget(data: { id: string; cameraTarget: ReadOnlyVector3 }) {
    futures[data.id].resolve(data.cameraTarget)
  },

  UserAcceptedCollectibles(data: { id: string }) {
    // Here, we should have "airdropObservable.notifyObservers(data.id)".
    // It's disabled because of security reasons.
  },

  EditAvatarClicked() {
    // We used to call delightedSurvey() here
  },

  ReportScene(sceneId: string) {
    browserInterface.OpenWebURL({ url: `https://decentralandofficial.typeform.com/to/KzaUxh?sceneId=${sceneId}` })
  },

  ReportPlayer(username: string) {
    browserInterface.OpenWebURL({ url: `https://decentralandofficial.typeform.com/to/owLkla?username=${username}` })
  },

  BlockPlayer(data: { userId: string }) {
    const profile = getProfile(globalThis.globalStore.getState(), identity.address)

    if (profile) {
      let blocked: string[] = [data.userId]

      if (profile.blocked) {
        for (let blockedUser of profile.blocked) {
          if (blockedUser === data.userId) {
            return
          }
        }

        // Merge the existing array and any previously blocked users
        blocked = [...profile.blocked, ...blocked]
      }

      globalThis.globalStore.dispatch(saveProfileRequest({ ...profile, blocked }))
    }
  },

  UnblockPlayer(data: { userId: string }) {
    const profile = getProfile(globalThis.globalStore.getState(), identity.address)

    if (profile) {
      const blocked = profile.blocked ? profile.blocked.filter((id) => id !== data.userId) : []
      globalThis.globalStore.dispatch(saveProfileRequest({ ...profile, blocked }))
    }
  },

  ReportUserEmail(data: { userEmail: string }) {
    const profile = getUserProfile().profile
    if (profile) {
      if (hasWallet) {
        window.analytics.identify(profile.userId, { email: data.userEmail })
      } else {
        window.analytics.identify({ email: data.userEmail })
      }
    }
  },

  RequestScenesInfoInArea(data: { parcel: { x: number; y: number }; scenesAround: number }) {
    globalThis.globalStore.dispatch(reportScenesAroundParcel(data.parcel, data.scenesAround))
  },

  SetAudioStream(data: { url: string; play: boolean; volume: number }) {
    setAudioStream(data.url, data.play, data.volume).catch((err) => defaultLogger.log(err))
  },

  SendChatMessage(data: { message: ChatMessage }) {
    globalThis.globalStore.dispatch(sendMessage(data.message))
  },
  async UpdateFriendshipStatus(message: FriendshipUpdateStatusMessage) {
    let { userId, action } = message

    // TODO - fix this hack: search should come from another message and method should only exec correct updates (userId, action) - moliva - 01/05/2020
    let found = false
    if (action === FriendshipAction.REQUESTED_TO) {
      await ProfileAsPromise(userId) // ensure profile
      found = hasConnectedWeb3(globalThis.globalStore.getState(), userId)
    }

    if (!found) {
      // if user profile was not found on server -> no connected web3, check if it's a claimed name
      const net = await getAppNetwork()
      const address = await fetchOwner(ethereumConfigurations[net].names, userId)
      if (address) {
        // if an address was found for the name -> set as user id & add that instead
        userId = address
        found = true
      }
    }

    if (action === FriendshipAction.REQUESTED_TO && !found) {
      // if we still haven't the user by now (meaning the user has never logged and doesn't have a profile in the dao, or the user id is for a non wallet user or name is not correct) -> fail
      // tslint:disable-next-line
      unityInterface.FriendNotFound(userId)
      return
    }

    globalThis.globalStore.dispatch(updateUserData(userId.toLowerCase(), toSocialId(userId)))
    globalThis.globalStore.dispatch(updateFriendship(action, userId.toLowerCase(), false))
  },

  async JumpIn(data: WorldPosition) {
    const {
      gridPosition: { x, y },
      realm: { serverName, layer }
    } = data

    const realmString = serverName + '-' + layer

    notifyStatusThroughChat(`Jumping to ${realmString} at ${x},${y}...`)

    const future = candidatesFetched()
    if (future.isPending) {
      notifyStatusThroughChat(`Waiting while realms are initialized, this may take a while...`)
    }

    await future

    const realm = changeRealm(realmString)

    if (realm) {
      catalystRealmConnected().then(
        () => {
          TeleportController.goTo(x, y, `Jumped to ${x},${y} in realm ${realmString}!`)
        },
        (e) => {
          const cause = e === 'realm-full' ? ' The requested realm is full.' : ''
          notifyStatusThroughChat('Could not join realm.' + cause)

          defaultLogger.error('Error joining realm', e)
        }
      )
    } else {
      notifyStatusThroughChat(`Couldn't find realm ${realmString}`)
    }
  },

  ScenesLoadingFeedback(data: { message: string; loadPercentage: number }) {
    const { message, loadPercentage } = data
    globalThis.globalStore.dispatch(updateStatusMessage(message, loadPercentage))
  }
}
globalThis.browserInterface2 = browserInterface
type BrowserInterfaceContainer = {
  browserInterface2: typeof browserInterface
}

function toSocialId(userId: string) {
  const domain = globalThis.globalStore.getState().chat.privateMessaging.client?.getDomain()
  return `@${userId.toLowerCase()}:${domain}`
}

export function setLoadingScreenVisible(shouldShow: boolean) {
  document.getElementById('overlay')!.style.display = shouldShow ? 'block' : 'none'
  document.getElementById('load-messages-wrapper')!.style.display = shouldShow ? 'block' : 'none'
  document.getElementById('progress-bar')!.style.display = shouldShow ? 'block' : 'none'
  const loadingAudio = document.getElementById('loading-audio') as HTMLMediaElement

  if (shouldShow) {
    loadingAudio?.play().catch((e) => {
      /*Ignored. If this fails is not critical*/
    })
  } else {
    loadingAudio?.pause()
  }

  if (!shouldShow && !EDITOR) {
    isTheFirstLoading = false
    TeleportController.stopTeleportAnimation()
  }
}

export function delightedSurvey() {
  // tslint:disable-next-line:strict-type-predicates
  if (typeof globalThis === 'undefined' || typeof globalThis !== 'object') {
    return
  }
  const { analytics, delighted } = globalThis
  if (!analytics || !delighted) {
    return
  }
  const profile = getUserProfile().profile as Profile | null
  if (!isTheFirstLoading && profile) {
    const payload = {
      email: profile.email || profile.ethAddress + '@dcl.gg',
      name: profile.name || 'Guest',
      properties: {
        ethAddress: profile.ethAddress,
        anonymous_id: analytics && analytics.user ? analytics.user().anonymousId() : null
      }
    }

    try {
      delighted.survey(payload)
    } catch (error) {
      defaultLogger.error('Delighted error: ' + error.message, error)
    }
  }
}

const CHUNK_SIZE = 100

export const unityInterface = {
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
  SetDisableAssetBundles() {
    gameInstance.SendMessage('SceneController', 'SetDisableAssetBundles')
  },
  ActivateRendering() {
    gameInstance.SendMessage('SceneController', 'ActivateRendering')
  },
  DeactivateRendering() {
    gameInstance.SendMessage('SceneController', 'DeactivateRendering')
  },
  UnlockCursor() {
    this.SetCursorState(false)
  },
  SetCursorState(locked: boolean) {
    gameInstance.SendMessage('MouseCatcher', 'UnlockCursorBrowser', locked ? 1 : 0)
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
    gameInstance.SendMessage('TutorialController', 'SetTutorialEnabled')
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
  RequestTeleport(teleportData: {}) {
    gameInstance.SendMessage('HUDController', 'RequestTeleport', JSON.stringify(teleportData))
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
  SetCameraPositionBuilder(position: Vector3) {
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

globalThis.unityInterface = unityInterface

export type UnityInterface = typeof unityInterface

export type UnityInterfaceContainer = {
  unityInterface: UnityInterface
}

////////////////////////////////////////////////////////////////////////////////

export const nativeMsgBridge: NativeMessagesBridge = new NativeMessagesBridge()
export const protobufMsgBridge: ProtobufMessagesBridge = new ProtobufMessagesBridge()

////////////////////////////////////////////////////////////////////////////////

/**
 *
 * Common initialization logic for the unity engine
 *
 * @param _gameInstance Unity game instance
 */
export async function initializeEngine(_gameInstance: GameInstance) {
  gameInstance = _gameInstance

  globalThis.globalStore.dispatch(unityClientLoaded())
  setLoadingScreenVisible(true)

  unityInterface.DeactivateRendering()

  if ( !WSS_ENABLED ) {
    nativeMsgBridge.initNativeMessages()
  }
  
  if (DEBUG) {
    unityInterface.SetDebug()
  }

  if (SCENE_DEBUG_PANEL) {
    unityInterface.SetSceneDebugPanel()
  }

  if (NO_ASSET_BUNDLES) {
    unityInterface.SetDisableAssetBundles()
  }

  if (SHOW_FPS_COUNTER) {
    unityInterface.ShowFPSPanel()
  }

  if (ENGINE_DEBUG_PANEL) {
    unityInterface.SetEngineDebugPanel()
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

export async function startUnityParcelLoading() {
  const p = await providerFuture
  hasWallet = p.successful

  globalThis.globalStore.dispatch(loadingScenes())
  await enableParcelSceneLoading({
    parcelSceneClass: UnityParcelScene,
    preloadScene: async (_land) => {
      // TODO:
      // 1) implement preload call
      // 2) await for preload message or timeout
      // 3) return
    },
    onLoadParcelScenes: (lands) => {
      unityInterface.LoadParcelScenes(
        lands.map(($) => {
          const x = Object.assign({}, ILandToLoadableParcelScene($).data)
          delete x.land
          return x
        })
      )
    },
    onUnloadParcelScenes: (lands) => {
      lands.forEach(($) => {
        unityInterface.UnloadScene($.sceneId)
      })
    },
    onPositionSettled: (spawnPoint) => {
      if (!aborted) {
        unityInterface.Teleport(spawnPoint)
        unityInterface.ActivateRendering()
      }
    },
    onPositionUnsettled: () => {
      unityInterface.DeactivateRendering()
    }
  })
}

async function initializeDecentralandUI() {
  const sceneId = 'dcl-ui-scene'

  const scene = new UnityScene({
    sceneId,
    name: 'ui',
    baseUrl: location.origin,
    main: hudWorkerUrl,
    useFPSThrottling: false,
    data: {},
    mappings: []
  })

  const worker = loadParcelScene(scene)
  worker.persistent = true

  await ensureUiApis(worker)

  unityInterface.CreateUIScene({ id: getParcelSceneID(scene), baseUrl: scene.data.baseUrl })
}

// Builder functions

let currentLoadedScene: SceneWorker | null

export async function loadPreviewScene() {
  const result = await fetch('/scene.json?nocache=' + Math.random())

  let lastId: string | null = null

  if (currentLoadedScene) {
    lastId = currentLoadedScene.parcelScene.data.sceneId
    stopParcelSceneWorker(currentLoadedScene)
  }

  if (result.ok) {
    // we load the scene to get the metadata
    // about rhe bounds and position of the scene
    // TODO(fmiras): Validate scene according to https://github.com/decentraland/proposals/blob/master/dsp/0020.mediawiki
    const scene = (await result.json()) as SceneJsonData
    const mappingsFetch = await fetch('/mappings')
    const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

    let defaultScene: ILand = {
      sceneId: 'previewScene',
      baseUrl: location.toString().replace(/\?[^\n]+/g, ''),
      baseUrlBundles: '',
      sceneJsonData: scene,
      mappingsResponse: mappingsResponse
    }

    const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(defaultScene))
    currentLoadedScene = loadParcelScene(parcelScene)

    const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(defaultScene).data }
    delete target.land

    defaultLogger.info('Reloading scene...')

    if (lastId) {
      unityInterface.UnloadScene(lastId)
    }

    unityInterface.LoadParcelScenes([target])

    defaultLogger.info('finish...')

    return defaultScene
  } else {
    throw new Error('Could not load scene.json')
  }
}

export function loadBuilderScene(sceneData: ILand) {
  unloadCurrentBuilderScene()

  const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(sceneData))
  currentLoadedScene = loadParcelScene(parcelScene)

  const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(sceneData).data }
  delete target.land

  unityInterface.LoadParcelScenes([target])
  return parcelScene
}

export function unloadCurrentBuilderScene() {
  if (currentLoadedScene) {
    const parcelScene = currentLoadedScene.parcelScene as UnityParcelScene
    parcelScene.emit('builderSceneUnloaded', {})

    stopParcelSceneWorker(currentLoadedScene)
    unityInterface.SendBuilderMessage('UnloadBuilderScene', parcelScene.data.sceneId)
    currentLoadedScene = null
  }
}

export function updateBuilderScene(sceneData: ILand) {
  if (currentLoadedScene) {
    const target: LoadableParcelScene = { ...ILandToLoadableParcelSceneUpdate(sceneData).data }
    delete target.land
    unityInterface.UpdateParcelScenes([target])
  }
}

teleportObservable.add((position: { x: number; y: number; text?: string }) => {
  // before setting the new position, show loading screen to avoid showing an empty world
  setLoadingScreenVisible(true)
  globalThis.globalStore.dispatch(teleportTriggered(position.text || `Teleporting to ${position.x}, ${position.y}`))
})

worldRunningObservable.add((isRunning) => {
  if (isRunning) {
    setLoadingScreenVisible(false)
  }
})

document.addEventListener('pointerlockchange', pointerLockChange, false)

let isPointerLocked: boolean = false
function pointerLockChange() {
  const doc: any = document
  const isLocked = (doc.pointerLockElement || doc.mozPointerLockElement || doc.webkitPointerLockElement) != null
  if (isPointerLocked !== isLocked) {
    unityInterface.SetCursorState(isLocked)
  }
  isPointerLocked = isLocked
}
