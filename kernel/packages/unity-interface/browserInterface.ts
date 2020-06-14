import { playerConfigurations } from 'config'
import { uuid } from 'decentraland-ecs/src'
import { ReadOnlyQuaternion, ReadOnlyVector3 } from 'decentraland-ecs/src/decentraland/math'
import { IEventNames } from 'decentraland-ecs/src/decentraland/Types'
import { sceneLifeCycleObservable } from 'decentraland-loader/lifecycle/controllers/scene'
import { tutorialStepId } from 'decentraland-loader/lifecycle/tutorial/tutorial'
import { identity } from 'shared'
import { queueTrackingEvent } from 'shared/analytics'
import { reportScenesAroundParcel } from 'shared/atlas/actions'
import { sendMessage, updateFriendship, updateUserData } from 'shared/chat/actions'
import { persistCurrentUser, sendPublicChatMessage } from 'shared/comms'
import { notifyStatusThroughChat } from 'shared/comms/chat'
import { AvatarMessageType } from 'shared/comms/interface/types'
import { avatarMessageObservable, getUserProfile } from 'shared/comms/peers'
import { candidatesFetched, catalystRealmConnected, changeRealm } from 'shared/dao/index'
import { globalDCL } from 'shared/globalDCL'
import { aborted } from 'shared/loading/ReportFatalError'
import { defaultLogger } from 'shared/logger'
import { saveProfileRequest } from 'shared/profiles/actions'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'
import { getProfile, hasConnectedWeb3 } from 'shared/profiles/selectors'
import { profileToRendererFormat } from 'shared/profiles/transformations/profileToRendererFormat'
import { Avatar, Profile } from 'shared/profiles/types'
import { browserInterfaceType } from 'shared/renderer-interface/browserInterface/browserInterfaceType'
import { Session } from 'shared/session'
import { getPerformanceInfo } from 'shared/session/getPerformanceInfo'
import { ChatMessage, FriendshipAction, FriendshipUpdateStatusMessage, WorldPosition } from 'shared/types'
import { getSceneWorkerBySceneID } from 'shared/world/parcelSceneManager'
import { positionObservable } from 'shared/world/positionThings'
import { TeleportController } from 'shared/world/TeleportController'
import { worldRunningObservable } from 'shared/world/worldState'
import { fetchOwner, toSocialId } from './getAddressByNameNFT'
import { cachedPositionEvent } from './position/setupPosition'
import { setAudioStream } from './audioStreamSource'
import { UnityParcelScene } from './dcl'
/////////////////////////////////// HANDLERS ///////////////////////////////////

export const browserInterface: browserInterfaceType = {
  /** Triggered when the camera moves */
  ReportPosition(data: { position: ReadOnlyVector3; rotation: ReadOnlyQuaternion; playerHeight?: number }) {
    cachedPositionEvent.position.set(data.position.x, data.position.y, data.position.z)
    cachedPositionEvent.quaternion.set(data.rotation.x, data.rotation.y, data.rotation.z, data.rotation.w)
    cachedPositionEvent.rotation.copyFrom(cachedPositionEvent.quaternion.eulerAngles)
    cachedPositionEvent.playerHeight = data.playerHeight || playerConfigurations.height
    positionObservable.notifyObservers(cachedPositionEvent)
  },

  ReportMousePosition(data: { id: string; mousePosition: ReadOnlyVector3 }) {
    cachedPositionEvent.mousePosition.set(data.mousePosition.x, data.mousePosition.y, data.mousePosition.z)
    positionObservable.notifyObservers(cachedPositionEvent)
    globalDCL.futures[data.id].resolve(data.mousePosition)
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
    if (globalDCL.hasWallet) {
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
    TeleportController.goToCrowd().catch(e => defaultLogger.error('error goToCrowd', e))
  },

  LogOut() {
    Session.current.then(s => s.logout()).catch(e => defaultLogger.error('error while logging out', e))
  },

  SaveUserAvatar(changes: { face: string; body: string; avatar: Avatar }) {
    const { face, body, avatar } = changes
    const profile: Profile = getUserProfile().profile as Profile
    const updated = { ...profile, avatar: { ...avatar, snapshots: { face, body } } }
    globalDCL.globalStore.dispatch(saveProfileRequest(updated))
  },

  SaveUserTutorialStep(data: { tutorialStep: number }) {
    const profile: Profile = getUserProfile().profile as Profile
    profile.tutorialStep = data.tutorialStep
    globalDCL.globalStore.dispatch(saveProfileRequest(profile))

    persistCurrentUser({
      version: profile.version,
      profile: profileToRendererFormat(profile, identity)
    })

    if (data.tutorialStep === tutorialStepId.FINISHED) {
      // we used to call delightedSurvey() here
    }
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
    globalDCL.futures[data.id].resolve(data.encodedTexture)
  },

  ReportBuilderCameraTarget(data: { id: string; cameraTarget: ReadOnlyVector3 }) {
    globalDCL.futures[data.id].resolve(data.cameraTarget)
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
    const profile = getProfile(globalDCL.globalStore.getState(), identity.address)

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

      globalDCL.globalStore.dispatch(saveProfileRequest({ ...profile, blocked }))
    }
  },

  UnblockPlayer(data: { userId: string }) {
    const profile = getProfile(globalDCL.globalStore.getState(), identity.address)

    if (profile) {
      const blocked = profile.blocked ? profile.blocked.filter(id => id !== data.userId) : []
      globalDCL.globalStore.dispatch(saveProfileRequest({ ...profile, blocked }))
    }
  },

  ReportUserEmail(data: { userEmail: string }) {
    const profile = getUserProfile().profile
    if (profile) {
      if (globalDCL.hasWallet) {
        globalDCL.analytics.identify(profile.userId, { email: data.userEmail })
      } else {
        globalDCL.analytics.identify({ email: data.userEmail })
      }
    }
  },

  RequestScenesInfoInArea(data: { parcel: { x: number; y: number }; scenesAround: number }) {
    globalDCL.globalStore.dispatch(reportScenesAroundParcel(data.parcel, data.scenesAround))
  },

  SetAudioStream(data: { url: string; play: boolean; volume: number }) {
    setAudioStream(data.url, data.play, data.volume).catch(err => defaultLogger.log(err))
  },

  SendChatMessage(data: { message: ChatMessage }) {
    globalDCL.globalStore.dispatch(sendMessage(data.message))
  },
  async UpdateFriendshipStatus(message: FriendshipUpdateStatusMessage) {
    let { userId, action } = message

    // TODO - fix this hack: search should come from another message and method should only exec correct updates (userId, action) - moliva - 01/05/2020
    let found = false
    if (action === FriendshipAction.REQUESTED_TO) {
      await ProfileAsPromise(userId) // ensure profile
      found = hasConnectedWeb3(globalDCL.globalStore.getState(), userId)
    }

    if (!found) {
      // if user profile was not found on server -> no connected web3, check if it's a claimed name
      const address = await fetchOwner(userId)
      if (address) {
        // if an address was found for the name -> set as user id & add that instead
        userId = address
        found = true
      }
    }

    if (action === FriendshipAction.REQUESTED_TO && !found) {
      // if we still haven't the user by now (meaning the user has never logged and doesn't have a profile in the dao, or the user id is for a non wallet user or name is not correct) -> fail
      // tslint:disable-next-line
      globalDCL.rendererInterface.FriendNotFound(userId)
      return
    }

    globalDCL.globalStore.dispatch(updateUserData(userId.toLowerCase(), toSocialId(userId)))
    globalDCL.globalStore.dispatch(updateFriendship(action, userId.toLowerCase(), false))
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
        e => {
          const cause = e === 'realm-full' ? ' The requested realm is full.' : ''
          notifyStatusThroughChat('Could not join realm.' + cause)

          defaultLogger.error('Error joining realm', e)
        }
      )
    } else {
      notifyStatusThroughChat(`Couldn't find realm ${realmString}`)
    }
  }
}
