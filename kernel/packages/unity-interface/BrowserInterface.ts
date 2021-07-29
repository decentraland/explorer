import { uuid } from 'atomicHelpers/math'
import { sendPublicChatMessage } from 'shared/comms'
import { AvatarMessageType } from 'shared/comms/interface/types'
import { avatarMessageObservable, localProfileUUID } from 'shared/comms/peers'
import { hasConnectedWeb3 } from 'shared/profiles/selectors'
import { TeleportController } from 'shared/world/TeleportController'
import { reportScenesAroundParcel } from 'shared/atlas/actions'
import { getCurrentIdentity, getCurrentUserId, getIsGuestLogin } from 'shared/session/selectors'
import {
  decentralandConfigurations,
  ethereumConfigurations,
  parcelLimits,
  playerConfigurations,
  WORLD_EXPLORER
} from 'config'
import { Quaternion, ReadOnlyQuaternion, ReadOnlyVector3, Vector3 } from 'decentraland-ecs'
import { IEventNames } from 'decentraland-ecs'
import { renderDistanceObservable, sceneLifeCycleObservable } from '../decentraland-loader/lifecycle/controllers/scene'
import { trackEvent } from 'shared/analytics'
import {
  BringDownClientAndShowError,
  ErrorContext,
  ReportFatalErrorWithUnityPayload
} from 'shared/loading/ReportFatalError'
import { defaultLogger } from 'shared/logger'
import { profileRequest, saveProfileRequest } from 'shared/profiles/actions'
import { Avatar, ProfileType } from 'shared/profiles/types'
import {
  ChatMessage,
  FriendshipUpdateStatusMessage,
  FriendshipAction,
  WorldPosition,
  LoadableParcelScene
} from 'shared/types'
import {
  getSceneWorkerBySceneID,
  setNewParcelScene,
  stopParcelSceneWorker,
  loadedSceneWorkers
} from 'shared/world/parcelSceneManager'
import { getPerformanceInfo } from 'shared/session/getPerformanceInfo'
import { positionObservable } from 'shared/world/positionThings'
import { sendMessage } from 'shared/chat/actions'
import { updateFriendship, updateUserData } from 'shared/friends/actions'
import { candidatesFetched, catalystRealmConnected, changeRealm } from 'shared/dao'
import { notifyStatusThroughChat } from 'shared/comms/chat'
import { fetchENSOwner, getAppNetwork } from 'shared/web3'
import { updateStatusMessage } from 'shared/loading/actions'
import { blockPlayers, mutePlayers, unblockPlayers, unmutePlayers } from 'shared/social/actions'
import { setAudioStream } from './audioStream'
import { logout, redirectToSignUp, signUp, signUpCancel, signupForm, signUpSetProfile } from 'shared/session/actions'
import { getIdentity, hasWallet } from 'shared/session'
import { getUnityInstance } from './IUnityInterface'
import { setDelightedSurveyEnabled } from './delightedSurvey'
import { IFuture } from 'fp-future'
import { reportHotScenes } from 'shared/social/hotScenes'
import { GIFProcessor } from 'gif-processor/processor'
import { setVoiceChatRecording, setVoicePolicy, setVoiceVolume, toggleVoiceChatRecording } from 'shared/comms/actions'
import { getERC20Balance } from 'shared/ethereum/EthereumService'
import { StatefulWorker } from 'shared/world/StatefulWorker'
import { ensureFriendProfile } from 'shared/friends/ensureFriendProfile'
import { reloadScene } from 'decentraland-loader/lifecycle/utils/reloadScene'
import { killPortableExperienceScene } from './portableExperiencesUtils'
import { wearablesRequest } from 'shared/catalogs/actions'
import { WearablesRequestFilters } from 'shared/catalogs/types'
import { fetchENSOwnerProfile } from './fetchENSOwnerProfile'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'
import { profileToRendererFormat } from 'shared/profiles/transformations/profileToRendererFormat'
import { AVATAR_LOADING_ERROR, renderingActivated, renderingDectivated } from 'shared/loading/types'
import { unpublishSceneByCoords } from 'shared/apis/SceneStateStorageController/unpublishScene'
import { BuilderServerAPIManager } from 'shared/apis/SceneStateStorageController/BuilderServerAPIManager'
import { areCandidatesFetched } from 'shared/dao/selectors'
import { openUrlObservable, signUpObservable } from 'shared/observables'
import { renderStateObservable } from 'shared/world/worldState'
import { realmToString } from 'shared/dao/utils/realmToString'
import { store } from 'shared/store/isolatedStore'

declare const globalThis: { gifProcessor?: GIFProcessor }
export let futures: Record<string, IFuture<any>> = {}

// ** TODO - move to friends related file - moliva - 15/07/2020
function toSocialId(userId: string) {
  const domain = store.getState().friends.client?.getDomain()
  return `@${userId.toLowerCase()}:${domain}`
}

const positionEvent = {
  position: Vector3.Zero(),
  quaternion: Quaternion.Identity,
  rotation: Vector3.Zero(),
  playerHeight: playerConfigurations.height,
  mousePosition: Vector3.Zero(),
  immediate: false // By default the renderer lerps avatars position
}

type SystemInfoPayload = {
  graphicsDeviceName: string
  graphicsDeviceVersion: string
  graphicsMemorySize: number
  processorType: string
  processorCount: number
  systemMemorySize: number
}

function allScenesEvent(data: { eventType: string; payload: any }) {
  for (const [_key, scene] of loadedSceneWorkers) {
    scene.emit(data.eventType as IEventNames, data.payload)
  }
}

// the BrowserInterface is a visitor for messages received from Unity
export class BrowserInterface {
  private lastBalanceOfMana: number = -1

  /**
   * This is the only method that should be called publically in this class.
   * It dispatches "renderer messages" to the correct handlers.
   *
   * It has a fallback that doesn't fail to support future versions of renderers
   * and independant workflows for both teams.
   */
  public handleUnityMessage(type: string, message: any) {
    if (type in this) {
      // tslint:disable-next-line:semicolon
      ;(this as any)[type](message)
    } else {
      defaultLogger.info(`Unknown message (did you forget to add ${type} to unity-interface/dcl.ts?)`, message)
    }
  }

  public AllScenesEvent(data: { eventType: string; payload: any }) {
    allScenesEvent(data)
  }

  /** Triggered when the camera moves */
  public ReportPosition(data: {
    position: ReadOnlyVector3
    rotation: ReadOnlyQuaternion
    playerHeight?: number
    immediate?: boolean
  }) {
    positionEvent.position.set(data.position.x, data.position.y, data.position.z)
    positionEvent.quaternion.set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w)
    positionEvent.rotation.copyFrom(positionEvent.quaternion.eulerAngles)
    positionEvent.playerHeight = data.playerHeight || playerConfigurations.height

    // By default the renderer lerps avatars position
    positionEvent.immediate = false

    if (data.immediate !== undefined) {
      positionEvent.immediate = data.immediate
    }

    positionObservable.notifyObservers(positionEvent)
  }

  public ReportMousePosition(data: { id: string; mousePosition: ReadOnlyVector3 }) {
    positionEvent.mousePosition.set(data.mousePosition.x, data.mousePosition.y, data.mousePosition.z)
    positionObservable.notifyObservers(positionEvent)
    futures[data.id].resolve(data.mousePosition)
  }

  public SceneEvent(data: { sceneId: string; eventType: string; payload: any }) {
    const scene = getSceneWorkerBySceneID(data.sceneId)
    if (scene) {
      scene.emit(data.eventType as IEventNames, data.payload)
    } else {
      if (data.eventType !== 'metricsUpdate') {
        defaultLogger.error(`SceneEvent: Scene ${data.sceneId} not found`, data)
      }
    }
  }

  public OpenWebURL(data: { url: string }) {
    openUrlObservable.notifyObservers(data)
  }

  public PerformanceReport(data: {
    samples: string
    fpsIsCapped: boolean
    hiccupsInThousandFrames: number
    hiccupsTime: number
    totalTime: number
  }) {
    const perfReport = getPerformanceInfo(data)
    trackEvent('performance report', perfReport)
  }

  public SystemInfoReport(data: SystemInfoPayload) {
    trackEvent('system info report', data)
  }

  public CrashPayloadResponse(data: { payload: any }) {
    getUnityInstance().crashPayloadResponseObservable.notifyObservers(JSON.stringify(data))
  }

  public PreloadFinished(data: { sceneId: string }) {
    // stub. there is no code about this in unity side yet
  }

  public Track(data: { name: string; properties: { key: string; value: string }[] | null }) {
    const properties: Record<string, string> = {}
    if (data.properties) {
      for (const property of data.properties) {
        properties[property.key] = property.value
      }
    }

    trackEvent(data.name, properties)
  }

  public TriggerExpression(data: { id: string; timestamp: number }) {
    avatarMessageObservable.notifyObservers({
      type: AvatarMessageType.USER_EXPRESSION,
      uuid: localProfileUUID || 'non-local-profile-uuid',
      expressionId: data.id,
      timestamp: data.timestamp
    })

    allScenesEvent({
      eventType: 'playerExpression',
      payload: {
        expressionId: data.id
      }
    })

    const messageId = uuid()
    const body = `␐${data.id} ${data.timestamp}`

    sendPublicChatMessage(messageId, body)
  }

  public TermsOfServiceResponse(data: { sceneId: string; accepted: boolean; dontShowAgain: boolean }) {
    // TODO
  }

  public MotdConfirmClicked() {
    if (hasWallet()) {
      TeleportController.goToNext()
    } else {
      openUrlObservable.notifyObservers({ url: 'https://docs.decentraland.org/get-a-wallet/' })
    }
  }

  public GoTo(data: { x: number; y: number }) {
    notifyStatusThroughChat(`Jumped to ${data.x},${data.y}!`)
    TeleportController.goTo(data.x, data.y)
  }

  public GoToMagic() {
    TeleportController.goToMagic()
  }

  public GoToCrowd() {
    TeleportController.goToCrowd().catch((e) => defaultLogger.error('error goToCrowd', e))
  }

  public LogOut() {
    store.dispatch(logout())
  }

  public RedirectToSignUp() {
    store.dispatch(redirectToSignUp())
  }

  public SaveUserInterests(interests: string[]) {
    if (!interests) {
      return
    }
    const unique = new Set<string>(interests)

    store.dispatch(saveProfileRequest({ interests: Array.from(unique) }))
  }

  public SaveUserAvatar(changes: {
    face: string
    face128: string
    face256: string
    body: string
    avatar: Avatar
    isSignUpFlow?: boolean
  }) {
    const { face, face128, face256, body, avatar } = changes
    const update = { avatar: { ...avatar, snapshots: { face, face128, face256, body } } }
    if (!changes.isSignUpFlow) {
      store.dispatch(saveProfileRequest(update))
    } else {
      store.dispatch(signUpSetProfile(update))
    }
  }

  public SendPassport(passport: { name: string; email: string }) {
    store.dispatch(signupForm(passport.name, passport.email))
    store.dispatch(signUp())
  }

  public RequestOwnProfileUpdate() {
    const userId = getCurrentUserId(store.getState())
    const isGuest = getIsGuestLogin(store.getState())
    if (!isGuest && userId) {
      store.dispatch(profileRequest(userId))
    }
  }

  public SaveUserUnverifiedName(changes: { newUnverifiedName: string }) {
    store.dispatch(saveProfileRequest({ unclaimedName: changes.newUnverifiedName }))
  }

  public CloseUserAvatar(isSignUpFlow = false) {
    if (isSignUpFlow) {
      getUnityInstance().DeactivateRendering()
      store.dispatch(signUpCancel())
    }
  }

  public SaveUserTutorialStep(data: { tutorialStep: number }) {
    const update = { tutorialStep: data.tutorialStep }
    store.dispatch(saveProfileRequest(update))
  }

  public ControlEvent({ eventType, payload }: { eventType: string; payload: any }) {
    switch (eventType) {
      case 'SceneReady': {
        const { sceneId } = payload
        sceneLifeCycleObservable.notifyObservers({ sceneId, status: 'ready' })
        break
      }
      case 'DeactivateRenderingACK': {
        /**
         * This event is called everytime the renderer deactivates its camera
         */
        store.dispatch(renderingDectivated())
        renderStateObservable.notifyObservers()
        break
      }
      case 'ActivateRenderingACK': {
        /**
         * This event is called everytime the renderer activates the main camera
         */
        store.dispatch(renderingActivated())
        renderStateObservable.notifyObservers()
        break
      }
      case 'StartStatefulMode': {
        const { sceneId } = payload
        const worker = getSceneWorkerBySceneID(sceneId)!
        getUnityInstance().UnloadScene(sceneId) // Maybe unity should do it by itself?
        const parcelScene = worker.getParcelScene()
        stopParcelSceneWorker(worker)
        const data = parcelScene.data.data as LoadableParcelScene
        getUnityInstance().LoadParcelScenes([data]) // Maybe unity should do it by itself?
        setNewParcelScene(sceneId, new StatefulWorker(parcelScene))
        break
      }
      case 'StopStatefulMode': {
        const { sceneId } = payload
        reloadScene(sceneId).catch((error) => defaultLogger.warn(`Failed to stop stateful mode`, error))
        break
      }
      default: {
        defaultLogger.warn(`Unknown event type ${eventType}, ignoring`)
        break
      }
    }
  }

  public SendScreenshot(data: { id: string; encodedTexture: string }) {
    futures[data.id].resolve(data.encodedTexture)
  }

  public ReportBuilderCameraTarget(data: { id: string; cameraTarget: ReadOnlyVector3 }) {
    futures[data.id].resolve(data.cameraTarget)
  }

  public UserAcceptedCollectibles(data: { id: string }) {
    // Here, we should have "airdropObservable.notifyObservers(data.id)".
    // It's disabled because of security reasons.
  }

  public SetDelightedSurveyEnabled(data: { enabled: boolean }) {
    setDelightedSurveyEnabled(data.enabled)
  }

  public SetScenesLoadRadius(data: { newRadius: number }) {
    parcelLimits.visibleRadius = Math.round(data.newRadius)

    renderDistanceObservable.notifyObservers({
      distanceInParcels: parcelLimits.visibleRadius
    })
  }

  public ReportScene(sceneId: string) {
    this.OpenWebURL({ url: `https://decentralandofficial.typeform.com/to/KzaUxh?sceneId=${sceneId}` })
  }

  public ReportPlayer(username: string) {
    this.OpenWebURL({ url: `https://decentralandofficial.typeform.com/to/owLkla?username=${username}` })
  }

  public BlockPlayer(data: { userId: string }) {
    store.dispatch(blockPlayers([data.userId]))
  }

  public UnblockPlayer(data: { userId: string }) {
    store.dispatch(unblockPlayers([data.userId]))
  }

  public ReportUserEmail(data: { userEmail: string }) {
    signUpObservable.notifyObservers({ email: data.userEmail })
  }

  public RequestScenesInfoInArea(data: { parcel: { x: number; y: number }; scenesAround: number }) {
    store.dispatch(reportScenesAroundParcel(data.parcel, data.scenesAround))
  }

  public SetAudioStream(data: { url: string; play: boolean; volume: number }) {
    setAudioStream(data.url, data.play, data.volume).catch((err) => defaultLogger.log(err))
  }

  public SendChatMessage(data: { message: ChatMessage }) {
    store.dispatch(sendMessage(data.message))
  }

  public SetVoiceChatRecording(recordingMessage: { recording: boolean }) {
    store.dispatch(setVoiceChatRecording(recordingMessage.recording))
  }

  public ToggleVoiceChatRecording() {
    store.dispatch(toggleVoiceChatRecording())
  }

  public ApplySettings(settingsMessage: { voiceChatVolume: number; voiceChatAllowCategory: number }) {
    store.dispatch(setVoiceVolume(settingsMessage.voiceChatVolume))
    store.dispatch(setVoicePolicy(settingsMessage.voiceChatAllowCategory))
  }

  public async UpdateFriendshipStatus(message: FriendshipUpdateStatusMessage) {
    let { userId, action } = message

    // TODO - fix this hack: search should come from another message and method should only exec correct updates (userId, action) - moliva - 01/05/2020
    let found = false
    if (action === FriendshipAction.REQUESTED_TO) {
      await ensureFriendProfile(userId)
      found = hasConnectedWeb3(store.getState(), userId)
    }

    if (!found) {
      // if user profile was not found on server -> no connected web3, check if it's a claimed name
      const net = await getAppNetwork()
      const address = await fetchENSOwner(ethereumConfigurations[net].names, userId)
      if (address) {
        // if an address was found for the name -> set as user id & add that instead
        userId = address
        found = true
      }
    }

    if (action === FriendshipAction.REQUESTED_TO && !found) {
      // if we still haven't the user by now (meaning the user has never logged and doesn't have a profile in the dao, or the user id is for a non wallet user or name is not correct) -> fail
      // tslint:disable-next-line
      getUnityInstance().FriendNotFound(userId)
      return
    }

    store.dispatch(updateUserData(userId.toLowerCase(), toSocialId(userId)))
    store.dispatch(updateFriendship(action, userId.toLowerCase(), false))
  }

  public SearchENSOwner(data: { name: string; maxResults?: number }) {
    const profilesPromise = fetchENSOwnerProfile(data.name, data.maxResults)

    profilesPromise
      .then((profiles) => {
        getUnityInstance().SetENSOwnerQueryResult(data.name, profiles)
      })
      .catch((error) => {
        getUnityInstance().SetENSOwnerQueryResult(data.name, undefined)
        defaultLogger.error(error)
      })
  }

  public async JumpIn(data: WorldPosition) {
    const {
      gridPosition: { x, y },
      realm: { serverName, layer }
    } = data

    const realmString = realmToString({ serverName, layer })

    notifyStatusThroughChat(`Jumping to ${realmString} at ${x},${y}...`)

    const future = candidatesFetched()

    if (!areCandidatesFetched(store.getState())) {
      notifyStatusThroughChat(`Waiting while realms are initialized, this may take a while...`)
    }

    await future

    const realm = changeRealm(realmString)

    if (realm) {
      catalystRealmConnected().then(
        () => {
          const successMessage = `Jumped to ${x},${y} in realm ${realmString}!`
          notifyStatusThroughChat(successMessage)
          getUnityInstance().ConnectionToRealmSuccess(data)
          TeleportController.goTo(x, y, successMessage)
        },
        (e) => {
          const cause = e === 'realm-full' ? ' The requested realm is full.' : ''
          notifyStatusThroughChat('Could not join realm.' + cause)
          getUnityInstance().ConnectionToRealmFailed(data)
          defaultLogger.error('Error joining realm', e)
        }
      )
    } else {
      notifyStatusThroughChat(`Couldn't find realm ${realmString}.`)
      getUnityInstance().ConnectionToRealmFailed(data)
    }
  }

  public ScenesLoadingFeedback(data: { message: string; loadPercentage: number }) {
    const { message, loadPercentage } = data
    store.dispatch(updateStatusMessage(message, loadPercentage))
  }

  public FetchHotScenes() {
    if (WORLD_EXPLORER) {
      reportHotScenes().catch((e: any) => {
        return defaultLogger.error('FetchHotScenes error', e)
      })
    }
  }

  public SetBaseResolution(data: { baseResolution: number }) {
    getUnityInstance().SetTargetHeight(data.baseResolution)
  }

  async RequestGIFProcessor(data: { imageSource: string; id: string; isWebGL1: boolean }) {
    if (!globalThis.gifProcessor) {
      globalThis.gifProcessor = new GIFProcessor(getUnityInstance().gameInstance, getUnityInstance(), data.isWebGL1)
    }

    globalThis.gifProcessor.ProcessGIF(data)
  }

  public DeleteGIF(data: { value: string }) {
    if (globalThis.gifProcessor) {
      globalThis.gifProcessor.DeleteGIF(data.value)
    }
  }

  public async FetchBalanceOfMANA() {
    const identity = getIdentity()

    if (!identity?.hasConnectedWeb3) {
      return
    }

    const balance = (await getERC20Balance(identity.address, decentralandConfigurations.paymentTokens.MANA)).toNumber()
    if (this.lastBalanceOfMana !== balance) {
      this.lastBalanceOfMana = balance
      getUnityInstance().UpdateBalanceOfMANA(`${balance}`)
    }
  }

  public SetMuteUsers(data: { usersId: string[]; mute: boolean }) {
    if (data.mute) {
      store.dispatch(mutePlayers(data.usersId))
    } else {
      store.dispatch(unmutePlayers(data.usersId))
    }
  }

  public async KillPortableExperience(data: { portableExperienceId: string }): Promise<void> {
    await killPortableExperienceScene(data.portableExperienceId)
  }

  public RequestBIWCatalogHeader() {
    const identity = getCurrentIdentity(store.getState())
    if (!identity) {
      let emptyHeader: Record<string, string> = {}
      getUnityInstance().SendBuilderCatalogHeaders(emptyHeader)
    } else {
      const headers = BuilderServerAPIManager.authorize(identity, 'get', '/assetpacks')
      getUnityInstance().SendBuilderCatalogHeaders(headers)
    }
  }

  public RequestWearables(data: {
    filters: {
      ownedByUser: string | null
      wearableIds?: string[] | null
      collectionIds?: string[] | null
    }
    context?: string
  }) {
    const { filters, context } = data
    const newFilters: WearablesRequestFilters = {
      ownedByUser: filters.ownedByUser ?? undefined,
      wearableIds: arrayCleanup(filters.wearableIds),
      collectionIds: arrayCleanup(filters.collectionIds)
    }
    store.dispatch(wearablesRequest(newFilters, context))
  }

  public RequestUserProfile(userIdPayload: { value: string }) {
    ProfileAsPromise(userIdPayload.value, undefined, ProfileType.DEPLOYED)
      .then((profile) => getUnityInstance().AddUserProfileToCatalog(profileToRendererFormat(profile)))
      .catch((error) => defaultLogger.error(`error fetching profile ${userIdPayload.value} ${error}`))
  }

  public ReportAvatarFatalError() {
    // TODO(Brian): Add more parameters?
    ReportFatalErrorWithUnityPayload(new Error(AVATAR_LOADING_ERROR), ErrorContext.RENDERER_AVATARS)
    BringDownClientAndShowError(AVATAR_LOADING_ERROR)
  }

  public UnpublishScene(data: { coordinates: string }) {
    unpublishSceneByCoords(data.coordinates).catch((error) => defaultLogger.log(error))
  }

  public async NotifyStatusThroughChat(data: { value: string }) {
    notifyStatusThroughChat(data.value)
  }
}

function arrayCleanup<T>(array: T[] | null | undefined): T[] | undefined {
  return !array || array.length === 0 ? undefined : array
}

export let browserInterface: BrowserInterface = new BrowserInterface()
