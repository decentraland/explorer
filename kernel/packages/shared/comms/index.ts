import { commConfigurations, parcelLimits, COMMS, AUTO_CHANGE_REALM, VOICE_CHAT_ENABLED } from 'config'
import { CommunicationsController } from 'shared/apis/CommunicationsController'
import { defaultLogger } from 'shared/logger'
import { ChatMessage as InternalChatMessage, ChatMessageType } from 'shared/types'
import { positionObservable, PositionReport, lastPlayerPosition } from 'shared/world/positionThings'
import { ProfileAsPromise } from '../profiles/ProfileAsPromise'
import { notifyStatusThroughChat } from './chat'
import { CliBrokerConnection } from './CliBrokerConnection'
import { Stats } from './debug'
import { IBrokerConnection } from '../comms/v1/IBrokerConnection'
import {
  getCurrentPeer,
  getPeer,
  getUser,
  localProfileUUID,
  receiveUserData,
  receiveUserPose,
  receiveUserVisible,
  removeById,
  avatarMessageObservable,
  receiveUserTalking
} from './peers'
import {
  Pose,
  UserInformation,
  Package,
  ChatMessage,
  ProfileVersion,
  BusMessage,
  AvatarMessageType,
  ConnectionEstablishmentError,
  IdTakenError,
  UnknownCommsModeError,
  VoiceFragment,
  ProfileType,
  ProfileRequest,
  ProfileResponse
} from './interface/types'
import {
  CommunicationArea,
  Position,
  position2parcel,
  sameParcel,
  squareDistance,
  ParcelArray,
  rotateUsingQuaternion
} from './interface/utils'
import { BrokerWorldInstanceConnection } from '../comms/v1/brokerWorldInstanceConnection'
import { profileToRendererFormat } from 'shared/profiles/transformations/profileToRendererFormat'
import { ProfileForRenderer, uuid } from 'decentraland-ecs/src'
import { worldRunningObservable, isWorldRunning, onNextWorldRunning } from '../world/worldState'
import { WorldInstanceConnection } from './interface/index'

import { LighthouseWorldInstanceConnection } from './v2/LighthouseWorldInstanceConnection'

import { Authenticator, AuthIdentity } from 'dcl-crypto'
import { getCommsServer, getRealm, getAllCatalystCandidates } from '../dao/selectors'
import { Realm, LayerUserInfo } from 'shared/dao/types'
import { Store } from 'redux'
import { RootState } from 'shared/store/rootTypes'
import { store } from 'shared/store/store'
import {
  setCatalystRealmCommsStatus,
  setCatalystRealm,
  markCatalystRealmFull,
  markCatalystRealmConnectionError
} from 'shared/dao/actions'
import { observeRealmChange, pickCatalystRealm, changeToCrowdedRealm } from 'shared/dao'
import { getCurrentUserProfile, getProfile } from 'shared/profiles/selectors'
import { Profile } from 'shared/profiles/types'
import { realmToString } from '../dao/utils/realmToString'
import { queueTrackingEvent } from 'shared/analytics'
import { messageReceived } from '../chat/actions'
import { arrayEquals } from 'atomicHelpers/arrayEquals'
import { getCommsConfig } from 'shared/meta/selectors'
import { ensureMetaConfigurationInitialized } from 'shared/meta/index'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import {
  NEW_LOGIN,
  UNEXPECTED_ERROR,
  commsEstablished,
  COMMS_COULD_NOT_BE_ESTABLISHED,
  commsErrorRetrying
} from 'shared/loading/types'
import { getIdentity, getStoredSession } from 'shared/session'
import { createLogger } from '../logger'
import { VoiceCommunicator, VoiceSpatialParams } from 'voice-chat-codec/VoiceCommunicator'
import { voicePlayingUpdate, voiceRecordingUpdate } from './actions'
import { isVoiceChatRecording } from './selectors'
import { VOICE_CHAT_SAMPLE_RATE } from 'voice-chat-codec/constants'
import future, { IFuture } from 'fp-future'

export type CommsVersion = 'v1' | 'v2'
export type CommsMode = CommsV1Mode | CommsV2Mode
export type CommsV1Mode = 'local' | 'remote'
export type CommsV2Mode = 'p2p' | 'server'

type Timestamp = number
type PeerAlias = string

export const MORDOR_POSITION: Position = [
  1000 * parcelLimits.parcelSize,
  1000,
  1000 * parcelLimits.parcelSize,
  0,
  0,
  0,
  0,
  true
]

type CommsContainer = {
  printCommsInformation: () => void
  bots: {
    create: () => string
    list: () => string[]
    remove: (id: string) => boolean
    reposition: (id: string) => void
  }
}

declare const globalThis: CommsContainer

const logger = createLogger('comms: ')

export class PeerTrackingInfo {
  public position: Position | null = null
  public identity: string | null = null
  public userInfo: UserInformation | null = null
  public lastPositionUpdate: Timestamp = 0
  public lastProfileUpdate: Timestamp = 0
  public lastUpdate: Timestamp = 0
  public receivedPublicChatMessages = new Set<string>()
  public talking = false

  profilePromise: {
    promise: Promise<ProfileForRenderer | void>
    version: number | null
    status: 'ok' | 'loading' | 'error'
  } = {
    promise: Promise.resolve(),
    version: null,
    status: 'loading'
  }

  profileType?: ProfileType

  public loadProfileIfNecessary(profileVersion: number) {
    if (this.identity && (profileVersion !== this.profilePromise.version || this.profilePromise.status === 'error')) {
      if (!this.userInfo || !this.userInfo.userId) {
        this.userInfo = {
          ...(this.userInfo || {}),
          userId: this.identity
        }
      }
      this.profilePromise = {
        promise: ProfileAsPromise(this.identity, profileVersion, this.profileType)
          .then((profile) => {
            const forRenderer = profileToRendererFormat(profile)
            this.lastProfileUpdate = new Date().getTime()
            const userInfo = this.userInfo || {}
            userInfo.version = profile.version
            this.userInfo = userInfo
            this.profilePromise.status = 'ok'
            return forRenderer
          })
          .catch((error) => {
            this.profilePromise.status = 'error'
            defaultLogger.error('Error fetching profile!', error)
          }),
        version: profileVersion,
        status: 'loading'
      }
    }
  }
}

export class Context {
  public readonly stats: Stats = new Stats(this)
  public commRadius: number

  public peerData = new Map<PeerAlias, PeerTrackingInfo>()
  public userInfo: UserInformation

  public currentPosition: Position | null = null

  public worldInstanceConnection: WorldInstanceConnection | null = null

  profileInterval?: NodeJS.Timer
  positionObserver: any
  worldRunningObserver: any
  infoCollecterInterval?: NodeJS.Timer
  analyticsInterval?: NodeJS.Timer

  timeToChangeRealm: number = Date.now() + commConfigurations.autoChangeRealmInterval

  positionUpdatesPaused: boolean = false

  constructor(userInfo: UserInformation) {
    this.userInfo = userInfo

    this.commRadius = commConfigurations.commRadius
  }
}

let context: Context | null = null
const scenesSubscribedToCommsEvents = new Set<CommunicationsController>()
let voiceCommunicator: VoiceCommunicator | null = null

/**
 * Returns a list of CIDs that must receive scene messages from comms
 */
function getParcelSceneSubscriptions(): string[] {
  let ids: string[] = []

  scenesSubscribedToCommsEvents.forEach(($) => {
    ids.push($.cid)
  })

  return ids
}

let audioRequestPending = false

function requestMediaDevice() {
  if (!audioRequestPending) {
    audioRequestPending = true
    // tslint:disable-next-line
    navigator.mediaDevices
      .getUserMedia({
        audio: {
          channelCount: 1,
          sampleRate: VOICE_CHAT_SAMPLE_RATE,
          echoCancellation: true,
          noiseSuppression: true,
          autoGainControl: true,
          advanced: [{ echoCancellation: true }, { autoGainControl: true }, { noiseSuppression: true }] as any
        },
        video: false
      })
      .then(async (a) => {
        await voiceCommunicator!.setInputStream(a)
        if (isVoiceChatRecording(store.getState())) {
          voiceCommunicator!.start()
        } else {
          voiceCommunicator!.pause()
        }
      })
      .catch((e) => {
        defaultLogger.log('Error requesting audio: ', e)
      })
      .finally(() => {
        audioRequestPending = false
      })
  }
}

export function updateVoiceRecordingStatus(recording: boolean) {
  if (!voiceCommunicator) {
    return
  }

  if (!recording) {
    voiceCommunicator.pause()
    return
  }

  if (!voiceCommunicator.hasInput()) {
    requestMediaDevice()
  } else {
    voiceCommunicator.start()
  }
}

export function updatePeerVoicePlaying(userId: string, playing: boolean) {
  if (context) {
    for (const peerInfo of context.peerData.values()) {
      if (peerInfo.identity === userId) {
        peerInfo.talking = playing
        break
      }
    }
  }
}

export function updateVoiceCommunicatorVolume(volume: number) {
  if (voiceCommunicator) {
    voiceCommunicator.setVolume(volume)
  }
}

export function sendPublicChatMessage(messageId: string, text: string) {
  if (context && context.currentPosition && context.worldInstanceConnection) {
    context.worldInstanceConnection
      .sendChatMessage(context.currentPosition, messageId, text)
      .catch((e) => defaultLogger.warn(`error while sending message `, e))
  }
}

export function sendParcelSceneCommsMessage(cid: string, message: string) {
  if (context && context.currentPosition && context.worldInstanceConnection) {
    context.worldInstanceConnection
      .sendParcelSceneCommsMessage(cid, message)
      .catch((e) => defaultLogger.warn(`error while sending message `, e))
  }
}

export function subscribeParcelSceneToCommsMessages(controller: CommunicationsController) {
  scenesSubscribedToCommsEvents.add(controller)
}

export function unsubscribeParcelSceneToCommsMessages(controller: CommunicationsController) {
  scenesSubscribedToCommsEvents.delete(controller)
}

const pendingProfileRequests: Record<string, IFuture<Profile>[]> = {}

export async function requestLocalProfileToPeers(userId: string, version?: number): Promise<Profile | null> {
  if (context && context.worldInstanceConnection && context.currentPosition) {
    if (!pendingProfileRequests[userId]) {
      pendingProfileRequests[userId] = []
    }

    const thisFuture = future<Profile>()

    pendingProfileRequests[userId].push(thisFuture)

    await context.worldInstanceConnection.sendProfileRequest(context.currentPosition, userId, version)

    setTimeout(function () {
      if (thisFuture.isPending) {
        thisFuture.reject(new Error('Profile request timed out for comms'))
        const pendingRequests = pendingProfileRequests[userId]
        if (pendingRequests && pendingRequests.includes(thisFuture)) {
          pendingRequests.splice(pendingRequests.indexOf(thisFuture), 1)
        }
      }
    }, 10000)

    return thisFuture
  } else {
    return Promise.resolve(null)
    // TODO: Maybe accumulate request to send after context is initialized?
  }
}

async function changeConnectionRealm(realm: Realm, url: string) {
  defaultLogger.log('Changing connection realm to ', JSON.stringify(realm), { url })
  if (context && context.worldInstanceConnection) {
    context.positionUpdatesPaused = true
    try {
      removeAllPeers(context)
      await sendToMordorAsync()
      await context.worldInstanceConnection.changeRealm(realm, url)
    } finally {
      context.positionUpdatesPaused = false
    }
  }
}

// TODO: Change ChatData to the new class once it is added to the .proto
export function processParcelSceneCommsMessage(context: Context, fromAlias: string, message: Package<ChatMessage>) {
  const { id: cid, text } = message.data

  const peer = getPeer(fromAlias)

  if (peer) {
    scenesSubscribedToCommsEvents.forEach(($) => {
      if ($.cid === cid) {
        $.receiveCommsMessage(text, peer)
      }
    })
  }
}

export function updateCommsUser(changes: Partial<UserInformation>) {
  const peer = getCurrentPeer()

  if (!peer || !localProfileUUID) throw new Error('cannotGetCurrentPeer')
  if (!peer.user) throw new Error('cannotGetCurrentPeer.user')

  Object.assign(peer.user, changes)

  receiveUserData(localProfileUUID, peer.user)

  const user = peer.user
  if (context) {
    if (user) {
      context.userInfo = user
    }
  }
}

function ensurePeerTrackingInfo(context: Context, alias: string): PeerTrackingInfo {
  let peerTrackingInfo = context.peerData.get(alias)

  if (!peerTrackingInfo) {
    peerTrackingInfo = new PeerTrackingInfo()
    context.peerData.set(alias, peerTrackingInfo)
  }
  return peerTrackingInfo
}

function processChatMessage(context: Context, fromAlias: string, message: Package<ChatMessage>) {
  const msgId = message.data.id
  const profile = getCurrentUserProfile(store.getState())

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)
  if (!peerTrackingInfo.receivedPublicChatMessages.has(msgId)) {
    const text = message.data.text
    peerTrackingInfo.receivedPublicChatMessages.add(msgId)

    const user = getUser(fromAlias)
    if (user) {
      if (text.startsWith('␐')) {
        const [id, timestamp] = text.split(' ')
        avatarMessageObservable.notifyObservers({
          type: AvatarMessageType.USER_EXPRESSION,
          uuid: fromAlias,
          expressionId: id.slice(1),
          timestamp: parseInt(timestamp, 10)
        })
      } else {
        if (profile && user.userId && !isBlocked(profile, user.userId)) {
          const messageEntry: InternalChatMessage = {
            messageType: ChatMessageType.PUBLIC,
            messageId: msgId,
            sender: user.userId || 'unknown',
            body: text,
            timestamp: message.time
          }
          store.dispatch(messageReceived(messageEntry))
        }
      }
    }
  }
}

function processVoiceFragment(context: Context, fromAlias: string, message: Package<VoiceFragment>) {
  const myAddress = getIdentity()?.address
  const profile = getCurrentUserProfile(store.getState())

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)

  if (peerTrackingInfo) {
    if (
      profile &&
      peerTrackingInfo.identity &&
      !isBlocked(profile, peerTrackingInfo.identity) &&
      !isMuted(profile, peerTrackingInfo.identity) &&
      !hasBlockedMe(myAddress, peerTrackingInfo.identity) &&
      peerTrackingInfo.position
    ) {
      voiceCommunicator?.playEncodedAudio(
        peerTrackingInfo.identity,
        getSpatialParamsFor(peerTrackingInfo.position),
        message.data.encoded,
        message.time
      )
    }
  }
}

function processProfileRequest(context: Context, fromAlias: string, message: Package<ProfileRequest>) {
  const myIdentity = getIdentity()
  const myAddress = myIdentity?.address

  if (message.data.userId !== myAddress) return

  ProfileAsPromise(
    myAddress,
    message.data.version ? parseInt(message.data.version) : undefined,
    myIdentity?.hasConnectedWeb3 ? ProfileType.DEPLOYED : ProfileType.LOCAL
  )
    .then((profile) => {
      if (context.currentPosition) {
        context.worldInstanceConnection?.sendProfileResponse(context.currentPosition, profile)
      }
    })
    .catch((e) => defaultLogger.error('Error getting profile for responding request to comms', e))
}

function processProfileResponse(context: Context, fromAlias: string, message: Package<ProfileResponse>) {
  const profile = message.data.profile

  if (pendingProfileRequests[profile.userId]) {
    pendingProfileRequests[profile.userId].forEach((it) => it.resolve(profile))
    delete pendingProfileRequests[profile.userId]
  }
}

function isBlocked(profile: Profile, userId: string): boolean {
  return !!profile.blocked && profile.blocked.includes(userId)
}

function isMuted(profile: Profile, userId: string): boolean {
  return !!profile.muted && profile.muted.includes(userId)
}

function hasBlockedMe(myAddress: string | undefined, theirAddress: string): boolean {
  const profile = getProfile(store.getState(), theirAddress)

  return !!profile && !!myAddress && isBlocked(profile, myAddress)
}

export function processProfileMessage(
  context: Context,
  fromAlias: string,
  peerIdentity: string,
  message: Package<ProfileVersion>
) {
  const msgTimestamp = message.time

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)

  if (msgTimestamp > peerTrackingInfo.lastProfileUpdate) {
    peerTrackingInfo.lastProfileUpdate = msgTimestamp
    peerTrackingInfo.identity = peerIdentity
    peerTrackingInfo.lastUpdate = Date.now()
    peerTrackingInfo.profileType = message.data.type

    if (ensureTrackingUniqueAndLatest(context, fromAlias, peerIdentity, msgTimestamp)) {
      const profileVersion = message.data.version
      peerTrackingInfo.loadProfileIfNecessary(profileVersion ? parseInt(profileVersion, 10) : 0)
    }
  }
}

/**
 * Ensures that there is only one peer tracking info for this identity.
 * Returns true if this is the latest update and the one that remains
 */
function ensureTrackingUniqueAndLatest(
  context: Context,
  fromAlias: string,
  peerIdentity: string,
  thisUpdateTimestamp: Timestamp
) {
  let currentLastProfileAlias = fromAlias
  let currentLastProfileUpdate = thisUpdateTimestamp

  context.peerData.forEach((info, key) => {
    if (info.identity === peerIdentity) {
      if (info.lastProfileUpdate < currentLastProfileUpdate) {
        removePeer(context, key)
      } else if (info.lastProfileUpdate > currentLastProfileUpdate) {
        removePeer(context, currentLastProfileAlias)
        currentLastProfileAlias = key
        currentLastProfileUpdate = info.lastProfileUpdate
      }
    }
  })

  return currentLastProfileAlias === fromAlias
}

export function processPositionMessage(context: Context, fromAlias: string, message: Package<Position>) {
  const msgTimestamp = message.time

  const peerTrackingInfo = ensurePeerTrackingInfo(context, fromAlias)

  const immediateReposition = message.data[7]
  if (immediateReposition || msgTimestamp > peerTrackingInfo.lastPositionUpdate) {
    const p = message.data

    peerTrackingInfo.position = p
    peerTrackingInfo.lastPositionUpdate = msgTimestamp
    peerTrackingInfo.lastUpdate = Date.now()
  }
}

type ProcessingPeerInfo = {
  alias: PeerAlias
  userInfo: UserInformation
  squareDistance: number
  position: Position
  talking: boolean
}

let currentParcelTopics = ''
let previousTopics = ''

let lastNetworkUpdatePosition = new Date().getTime()
let lastPositionSent: Position | undefined

export function onPositionUpdate(context: Context, p: Position) {
  const worldConnection = context.worldInstanceConnection

  if (!worldConnection || !worldConnection.isAuthenticated) {
    return
  }

  const oldParcel = context.currentPosition ? position2parcel(context.currentPosition) : null
  const newParcel = position2parcel(p)
  const immediateReposition = p[7]

  if (!sameParcel(oldParcel, newParcel)) {
    const commArea = new CommunicationArea(newParcel, context.commRadius)

    const xMin = ((commArea.vMin.x + parcelLimits.maxParcelX) >> 2) << 2
    const xMax = ((commArea.vMax.x + parcelLimits.maxParcelX) >> 2) << 2
    const zMin = ((commArea.vMin.z + parcelLimits.maxParcelZ) >> 2) << 2
    const zMax = ((commArea.vMax.z + parcelLimits.maxParcelZ) >> 2) << 2

    let rawTopics: string[] = []
    for (let x = xMin; x <= xMax; x += 4) {
      for (let z = zMin; z <= zMax; z += 4) {
        const hash = `${x >> 2}:${z >> 2}`
        if (!rawTopics.includes(hash)) {
          rawTopics.push(hash)
        }
      }
    }
    currentParcelTopics = rawTopics.join(' ')
    if (context.currentPosition && !context.positionUpdatesPaused) {
      worldConnection
        .sendParcelUpdateMessage(context.currentPosition, p)
        .catch((e) => defaultLogger.warn(`error while sending message `, e))
    }
  }

  if (!immediateReposition) {
    // Otherwise the topics get lost on an immediate reposition...
    const parcelSceneSubscriptions = getParcelSceneSubscriptions()
    const parcelSceneCommsTopics = parcelSceneSubscriptions.join(' ')

    const topics =
      (context.userInfo.userId ? context.userInfo.userId + ' ' : '') +
      currentParcelTopics +
      (parcelSceneCommsTopics.length ? ' ' + parcelSceneCommsTopics : '')

    if (topics !== previousTopics) {
      worldConnection
        .updateSubscriptions(topics.split(' '))
        .catch((e) => defaultLogger.warn(`error while updating subscriptions`, e))
      previousTopics = topics
    }
  }

  context.currentPosition = p

  voiceCommunicator?.setListenerSpatialParams(getSpatialParamsFor(context.currentPosition))

  const now = Date.now()
  const elapsed = now - lastNetworkUpdatePosition

  // We only send the same position message as a ping if we have not sent positions in the last 5 seconds
  if (!immediateReposition && arrayEquals(p, lastPositionSent) && elapsed < 5000) {
    return
  }

  if ((immediateReposition || elapsed > 100) && !context.positionUpdatesPaused) {
    lastPositionSent = p
    lastNetworkUpdatePosition = now
    worldConnection.sendPositionMessage(p).catch((e) => defaultLogger.warn(`error while sending message `, e))
  }
}

function getSpatialParamsFor(position: Position): VoiceSpatialParams {
  return {
    position: position.slice(0, 3) as [number, number, number],
    orientation: rotateUsingQuaternion(position, 0, 0, -1)
  }
}

function collectInfo(context: Context) {
  if (context.stats) {
    context.stats.collectInfoDuration.start()
  }

  if (!context.currentPosition) {
    return
  }

  const now = Date.now()
  const visiblePeers: ProcessingPeerInfo[] = []
  const commArea = new CommunicationArea(position2parcel(context.currentPosition), commConfigurations.commRadius)
  for (let [peerAlias, trackingInfo] of context.peerData) {
    const msSinceLastUpdate = now - trackingInfo.lastUpdate

    if (msSinceLastUpdate > commConfigurations.peerTtlMs) {
      removePeer(context, peerAlias)

      continue
    }

    if (trackingInfo.identity === getIdentity()?.address) {
      // If we are tracking a peer that is ourselves, we remove it
      removePeer(context, peerAlias)
      continue
    }

    if (!trackingInfo.position || !trackingInfo.userInfo) {
      continue
    }

    if (!commArea.contains(trackingInfo.position)) {
      receiveUserVisible(peerAlias, false)
      continue
    }

    visiblePeers.push({
      position: trackingInfo.position,
      userInfo: trackingInfo.userInfo,
      squareDistance: squareDistance(context.currentPosition, trackingInfo.position),
      alias: peerAlias,
      talking: trackingInfo.talking
    })
  }

  if (visiblePeers.length <= commConfigurations.maxVisiblePeers) {
    for (let peerInfo of visiblePeers) {
      const alias = peerInfo.alias
      receiveUserVisible(alias, true)
      receiveUserPose(alias, peerInfo.position as Pose)
      receiveUserData(alias, peerInfo.userInfo)
      receiveUserTalking(alias, peerInfo.talking)
    }
  } else {
    const sortedBySqDistanceVisiblePeers = visiblePeers.sort((p1, p2) => p1.squareDistance - p2.squareDistance)
    for (let i = 0; i < sortedBySqDistanceVisiblePeers.length; ++i) {
      const peer = sortedBySqDistanceVisiblePeers[i]
      const alias = peer.alias

      if (i < commConfigurations.maxVisiblePeers) {
        receiveUserVisible(alias, true)
        receiveUserPose(alias, peer.position as Pose)
        receiveUserData(alias, peer.userInfo)
        receiveUserTalking(alias, peer.talking)
      } else {
        receiveUserVisible(alias, false)
      }
    }
  }

  checkAutochangeRealm(visiblePeers, context, now)

  if (context.stats) {
    context.stats.visiblePeerIds = visiblePeers.map((it) => it.alias)
    context.stats.trackingPeersCount = context.peerData.size
    context.stats.collectInfoDuration.stop()
  }
}

function checkAutochangeRealm(visiblePeers: ProcessingPeerInfo[], context: Context, now: number) {
  if (AUTO_CHANGE_REALM) {
    if (visiblePeers.length > 0) {
      context.timeToChangeRealm = now + commConfigurations.autoChangeRealmInterval
    } else if (now > context.timeToChangeRealm) {
      context.timeToChangeRealm = now + commConfigurations.autoChangeRealmInterval
      defaultLogger.log('Changing to crowded realm because there is no people around')
      changeToCrowdedRealm().then(
        ([changed, realm]) => {
          if (changed) {
            defaultLogger.log('Successfully changed to realm', realm)
          } else {
            defaultLogger.log('No crowded realm found')
          }
        },
        (error) => defaultLogger.warn('Error trying to change realm', error)
      )
    }
  }
}

function removeAllPeers(context: Context) {
  for (const alias of context.peerData.keys()) {
    removePeer(context, alias)
  }
}

function removePeer(context: Context, peerAlias: string) {
  context.peerData.delete(peerAlias)
  removeById(peerAlias)
  if (context.stats) {
    context.stats.onPeerRemoved(peerAlias)
  }
}

function parseCommsMode(modeString: string) {
  const segments = modeString.split('-')
  return segments as [CommsVersion, CommsMode]
}

const NOOP = () => {
  // do nothing
}
function subscribeToRealmChange(store: Store<RootState>) {
  observeRealmChange(store, (previousRealm, currentRealm) =>
    changeConnectionRealm(currentRealm, getCommsServer(store.getState())).then(NOOP, defaultLogger.error)
  )
}

export async function connect(userId: string) {
  try {
    const user = getStoredSession(userId)
    if (!user) {
      return undefined
    }

    const userInfo = {
      ...user
    }

    let connection: WorldInstanceConnection

    const [version, mode] = parseCommsMode(COMMS)
    switch (version) {
      case 'v1': {
        let commsBroker: IBrokerConnection

        switch (mode) {
          case 'local': {
            let location = document.location.toString()
            if (location.indexOf('#') > -1) {
              location = location.substring(0, location.indexOf('#')) // drop fragment identifier
            }
            const commsUrl = location.replace(/^http/, 'ws') // change protocol to ws

            const url = new URL(commsUrl)
            const qs = new URLSearchParams({
              identity: btoa(userId)
            })
            url.search = qs.toString()

            defaultLogger.log('Using WebSocket comms: ' + url.href)
            commsBroker = new CliBrokerConnection(url.href)
            break
          }
          default: {
            throw new UnknownCommsModeError(`unrecognized mode for comms v1 "${mode}"`)
          }
        }

        const instance = new BrokerWorldInstanceConnection(commsBroker)
        await instance.isConnected
        store.dispatch(commsEstablished())

        connection = instance
        break
      }
      case 'v2': {
        await ensureMetaConfigurationInitialized()
        const lighthouseUrl = getCommsServer(store.getState())
        const realm = getRealm(store.getState())
        const commsConfig = getCommsConfig(store.getState())

        const peerConfig: any = {
          connectionConfig: {
            iceServers: commConfigurations.iceServers
          },
          authHandler: async (msg: string) => {
            try {
              return Authenticator.signPayload(getIdentity() as AuthIdentity, msg)
            } catch (e) {
              defaultLogger.info(`error while trying to sign message from lighthouse '${msg}'`)
            }
            // if any error occurs
            return getIdentity()
          },
          logLevel: 'NONE',
          targetConnections: commsConfig.targetConnections ?? 4,
          maxConnections: commsConfig.maxConnections ?? 6,
          positionConfig: {
            selfPosition: () => {
              if (context && context.currentPosition) {
                return context.currentPosition.slice(0, 3)
              }
            },
            maxConnectionDistance: 4,
            nearbyPeersDistance: 5,
            disconnectDistance: 5
          }
        }

        if (!commsConfig.relaySuspensionDisabled) {
          peerConfig.relaySuspensionConfig = {
            relaySuspensionInterval: commsConfig.relaySuspensionInterval ?? 750,
            relaySuspensionDuration: commsConfig.relaySuspensionDuration ?? 5000
          }
        }

        defaultLogger.log('Using Remote lighthouse service: ', lighthouseUrl)

        connection = new LighthouseWorldInstanceConnection(
          getIdentity()?.address as string,
          realm!,
          lighthouseUrl,
          peerConfig,
          (status) => {
            store.dispatch(setCatalystRealmCommsStatus(status))
            switch (status.status) {
              case 'realm-full': {
                handleFullLayer()
                break
              }
              case 'reconnection-error': {
                handleReconnectionError()
                break
              }
            }
          }
        )

        break
      }
      default: {
        throw new Error(`unrecognized comms mode "${COMMS}"`)
      }
    }

    subscribeToRealmChange(store)

    context = new Context(userInfo)
    context.worldInstanceConnection = connection

    if (isWorldRunning()) {
      await startCommunications(context)
    } else {
      onNextWorldRunning(async () => {
        try {
          await startCommunications(context!)
        } catch (e) {
          disconnect()
          if (e instanceof IdTakenError) {
            ReportFatalError(NEW_LOGIN)
          } else {
            // not a comms issue per se => rethrow error
            defaultLogger.error(`error while trying to establish communications `, e)
            ReportFatalError(UNEXPECTED_ERROR)
          }
        }
      })
    }

    return context
  } catch (e) {
    defaultLogger.error(e)
    if (e instanceof IdTakenError) {
      throw e
    } else {
      throw new ConnectionEstablishmentError(e.message)
    }
  }
}

export async function startCommunications(context: Context) {
  const maxAttemps = 5
  for (let i = 1; ; ++i) {
    try {
      logger.info(`Attempt number ${i}...`)
      await doStartCommunications(context)

      break
    } catch (e) {
      if (e instanceof IdTakenError) {
        disconnect()
        ReportFatalError(NEW_LOGIN)
        throw e
      } else if (e instanceof ConnectionEstablishmentError) {
        if (i >= maxAttemps) {
          // max number of attemps reached => rethrow error
          logger.info(`Max number of attemps reached (${maxAttemps}), unsuccessful connection`)
          disconnect()
          ReportFatalError(COMMS_COULD_NOT_BE_ESTABLISHED)
          throw e
        } else {
          // max number of attempts not reached => continue with loop
          store.dispatch(commsErrorRetrying(i))
        }
      } else {
        // not a comms issue per se => rethrow error
        logger.error(`error while trying to establish communications `, e)
        disconnect()
        const realm = getRealm(store.getState())
        store.dispatch(markCatalystRealmConnectionError(realm!))
      }
    }
  }
}

async function doStartCommunications(context: Context) {
  const connection = context.worldInstanceConnection!
  try {
    try {
      if (connection instanceof LighthouseWorldInstanceConnection) {
        await connection.connectPeer()
        store.dispatch(commsEstablished())
      }
    } catch (e) {
      // Do nothing if layer is full. This will be handled by status handler
      if (!(e.responseJson && e.responseJson.status === 'layer_is_full')) {
        throw e
      }
    }

    connection.positionHandler = (alias: string, data: Package<Position>) => {
      processPositionMessage(context, alias, data)
    }
    connection.profileHandler = (alias: string, identity: string, data: Package<ProfileVersion>) => {
      processProfileMessage(context, alias, identity, data)
    }
    connection.chatHandler = (alias: string, data: Package<ChatMessage>) => {
      processChatMessage(context, alias, data)
    }
    connection.sceneMessageHandler = (alias: string, data: Package<BusMessage>) => {
      processParcelSceneCommsMessage(context, alias, data)
    }
    connection.voiceHandler = (alias: string, data: Package<VoiceFragment>) => {
      processVoiceFragment(context, alias, data)
    }
    connection.profileRequestHandler = (alias: string, data: Package<ProfileRequest>) => {
      processProfileRequest(context, alias, data)
    }
    connection.profileResponseHandler = (alias: string, data: Package<ProfileResponse>) => {
      processProfileResponse(context, alias, data)
    }

    if (commConfigurations.debug) {
      connection.stats = context.stats
    }

    context.profileInterval = setInterval(() => {
      if (context && context.currentPosition && context.worldInstanceConnection) {
        context.worldInstanceConnection
          .sendProfileMessage(context.currentPosition, context.userInfo)
          .catch((e) => defaultLogger.warn(`error while sending message `, e))
      }
    }, 1000)

    if (commConfigurations.sendAnalytics) {
      context.analyticsInterval = setInterval(() => {
        const connectionAnalytics = connection.analyticsData()
        // We slice the ids in order to reduce the potential event size. Eventually, we should slice all comms ids
        connectionAnalytics.trackedPeers = context?.peerData.keys()
          ? [...context?.peerData.keys()].map((it) => it.slice(-6))
          : []
        connectionAnalytics.visiblePeers = context?.stats.visiblePeerIds.map((it) => it.slice(-6))

        if (connectionAnalytics) {
          queueTrackingEvent('Comms Status v2', connectionAnalytics)
        }
      }, 30000)
    }

    context.worldRunningObserver = worldRunningObservable.add((isRunning) => {
      onWorldRunning(isRunning)
    })

    context.positionObserver = positionObservable.add((obj: Readonly<PositionReport>) => {
      const p = [
        obj.position.x,
        obj.position.y - obj.playerHeight,
        obj.position.z,
        obj.quaternion.x,
        obj.quaternion.y,
        obj.quaternion.z,
        obj.quaternion.w,
        obj.immediate
      ] as Position

      if (context && isWorldRunning) {
        onPositionUpdate(context, p)
      }
    })

    window.addEventListener('beforeunload', () => {
      context.positionUpdatesPaused = true
      sendToMordor()
    })

    context.infoCollecterInterval = setInterval(() => {
      if (context) {
        collectInfo(context)
      }
    }, 100)

    if (!voiceCommunicator && VOICE_CHAT_ENABLED) {
      voiceCommunicator = new VoiceCommunicator(
        context.userInfo.userId!,
        {
          send(data: Uint8Array) {
            if (context.currentPosition) {
              context.worldInstanceConnection?.sendVoiceMessage(context.currentPosition, data)
            }
          }
        },

        {
          initialListenerParams: context.currentPosition ? getSpatialParamsFor(context.currentPosition) : undefined,
          panningModel: commConfigurations.voiceChatUseHRTF ? 'HRTF' : 'equalpower',
          loopbackAudioElement: document.getElementById('voice-chat-audio') as HTMLAudioElement | undefined
        }
      )

      voiceCommunicator.addStreamPlayingListener((userId, playing) => {
        store.dispatch(voicePlayingUpdate(userId, playing))
      })

      voiceCommunicator.addStreamRecordingListener((recording) => {
        store.dispatch(voiceRecordingUpdate(recording))
      })
    }
  } catch (e) {
    if (e.message && e.message.includes('is taken')) {
      throw new IdTakenError(e.message)
    } else {
      throw new ConnectionEstablishmentError(e.message)
    }
  }
}

function handleReconnectionError() {
  const realm = getRealm(store.getState())

  if (realm) {
    store.dispatch(markCatalystRealmConnectionError(realm))
  }

  const candidates = getAllCatalystCandidates(store.getState())

  const otherRealm = pickCatalystRealm(candidates)

  const notificationMessage = realm
    ? `Lost connection to ${realmToString(realm)}, joining realm ${realmToString(otherRealm)} instead`
    : `Joining realm ${realmToString(otherRealm)}`

  notifyStatusThroughChat(notificationMessage)

  store.dispatch(setCatalystRealm(otherRealm))
}

function handleFullLayer() {
  const realm = getRealm(store.getState())

  if (realm) {
    store.dispatch(markCatalystRealmFull(realm))
  }

  const candidates = getAllCatalystCandidates(store.getState())

  const otherRealm = pickCatalystRealm(candidates)

  notifyStatusThroughChat(
    `Joining realm ${otherRealm.catalystName}-${otherRealm.layer} since the previously requested was full`
  )

  store.dispatch(setCatalystRealm(otherRealm))
}

export function onWorldRunning(isRunning: boolean, _context: Context | null = context) {
  if (!isRunning) {
    sendToMordor(_context)
  }
}

export function sendToMordor(_context: Context | null = context) {
  sendToMordorAsync().catch((e) => defaultLogger.warn(`error while sending message `, e))
}

async function sendToMordorAsync(_context: Context | null = context) {
  if (_context && _context.worldInstanceConnection && _context.currentPosition) {
    await _context.worldInstanceConnection.sendParcelUpdateMessage(_context.currentPosition, MORDOR_POSITION)
  }
}

export function disconnect() {
  if (context) {
    if (context.profileInterval) {
      clearInterval(context.profileInterval)
    }
    if (context.infoCollecterInterval) {
      clearInterval(context.infoCollecterInterval)
    }
    if (context.analyticsInterval) {
      clearInterval(context.analyticsInterval)
    }
    if (context.positionObserver) {
      positionObservable.remove(context.positionObserver)
    }
    if (context.worldRunningObserver) {
      worldRunningObservable.remove(context.worldRunningObserver)
    }
    if (context.worldInstanceConnection) {
      context.worldInstanceConnection.close()
    }
  }
}

export async function fetchLayerUsersParcels(): Promise<ParcelArray[]> {
  const realm = getRealm(store.getState())
  const commsUrl = getCommsServer(store.getState())

  if (realm && realm.layer && commsUrl) {
    const layerUsersResponse = await fetch(`${commsUrl}/layers/${realm.layer}/users`)
    if (layerUsersResponse.ok) {
      const layerUsers: LayerUserInfo[] = await layerUsersResponse.json()
      return layerUsers.filter((it) => it.parcel).map((it) => it.parcel!)
    }
  }

  return []
}

globalThis.printCommsInformation = function () {
  if (context) {
    defaultLogger.log('Communication topics: ' + previousTopics)
    context.stats.printDebugInformation()
  }
}

type Bot = { id: string; handle: any }
const bots: Bot[] = []

globalThis.bots = {
  create: () => {
    const id = uuid()
    processProfileMessage(context!, id, id, {
      type: 'profile',
      time: Date.now(),
      data: {
        version: '1',
        user: id,
        type: ProfileType.DEPLOYED
      }
    })
    const position = { ...lastPlayerPosition }
    const handle = setInterval(() => {
      processPositionMessage(context!, id, {
        type: 'position',
        time: Date.now(),
        data: [position.x, position.y, position.z, 0, 0, 0, 0, false]
      })
    }, 1000)
    bots.push({ id, handle })
    return id
  },
  remove: (id: string | undefined) => {
    let bot
    if (id) {
      bot = bots.find((bot) => bot.id === id)
    } else {
      bot = bots.length > 0 ? bots[0] : undefined
    }
    if (bot) {
      clearInterval(bot.handle)
      bots.splice(bots.indexOf(bot), 1)
      return true
    }
    return false
  },
  reposition: (id: string) => {
    // to test immediate repositioning
    let bot = bots.find((bot) => bot.id === id)
    if (bot) {
      const position = { ...lastPlayerPosition }

      bot.handle = processPositionMessage(context!, id, {
        type: 'position',
        time: Date.now(),
        data: [position.x, position.y, position.z, 0, 0, 0, 0, true]
      })
    }
  },
  list: () => bots.map((bot) => bot.id)
}
