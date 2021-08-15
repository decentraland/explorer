import { put, takeEvery, select, call, takeLatest } from 'redux-saga/effects'

import { EDITOR } from 'config'

import { establishingComms, FATAL_ERROR } from 'shared/loading/types'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { getCurrentIdentity } from 'shared/session/selectors'
import { setWorldContext } from 'shared/protocol/actions'
import { waitForRealmInitialized, selectRealm } from 'shared/dao/sagas'
import { getRealm } from 'shared/dao/selectors'
import { CATALYST_REALMS_SCAN_SUCCESS, setCatalystRealm } from 'shared/dao/actions'
import { Realm } from 'shared/dao/types'
import { realmToString } from 'shared/dao/utils/realmToString'
import { createLogger } from 'shared/logger'

import {
  connect,
  Context,
  disconnect,
  updatePeerVoicePlaying,
  updateVoiceCommunicatorMute,
  updateVoiceCommunicatorVolume,
  updateVoiceRecordingStatus
} from '.'
import {
  SetVoiceMute,
  SetVoiceVolume,
  SET_VOICE_CHAT_RECORDING,
  SET_VOICE_MUTE,
  SET_VOICE_VOLUME,
  TOGGLE_VOICE_CHAT_RECORDING,
  VoicePlayingUpdate,
  VoiceRecordingUpdate,
  VOICE_PLAYING_UPDATE,
  VOICE_RECORDING_UPDATE
} from './actions'

import { isVoiceChatRecording } from './selectors'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { sceneObservable } from 'shared/world/sceneState'
import { SceneFeatureToggles } from 'shared/types'
import { isFeatureToggleEnabled } from 'shared/selectors'
import { waitForRendererInstance } from 'shared/renderer/sagas'

const DEBUG = false
const logger = createLogger('comms: ')

export function* commsSaga() {
  yield takeEvery(USER_AUTHENTIFIED, userAuthentified)
  yield takeLatest(CATALYST_REALMS_SCAN_SUCCESS, changeRealm)
  yield takeEvery(FATAL_ERROR, bringDownComms)
}

function* bringDownComms() {
  disconnect()
}

function* listenToWhetherSceneSupportsVoiceChat() {
  sceneObservable.add(({ previousScene, newScene }) => {
    const previouslyEnabled = previousScene
      ? isFeatureToggleEnabled(SceneFeatureToggles.VOICE_CHAT, previousScene.sceneJsonData)
      : undefined
    const nowEnabled = newScene
      ? isFeatureToggleEnabled(SceneFeatureToggles.VOICE_CHAT, newScene.sceneJsonData)
      : undefined
    if (previouslyEnabled !== nowEnabled && nowEnabled !== undefined) {
      getUnityInstance().SetVoiceChatEnabledByScene(nowEnabled)
      if (!nowEnabled) {
        // We want to stop any potential recordings when a user enters a new scene
        updateVoiceRecordingStatus(false)
      }
    }
  })
}

function* userAuthentified() {
  if (EDITOR) {
    return
  }

  yield call(waitForRealmInitialized)

  const identity = yield select(getCurrentIdentity)

  yield takeEvery(SET_VOICE_CHAT_RECORDING, updateVoiceChatRecordingStatus)
  yield takeEvery(TOGGLE_VOICE_CHAT_RECORDING, updateVoiceChatRecordingStatus)
  yield takeEvery(VOICE_PLAYING_UPDATE, updateUserVoicePlaying)
  yield takeEvery(VOICE_RECORDING_UPDATE, updatePlayerVoiceRecording)
  yield takeEvery(SET_VOICE_VOLUME, updateVoiceChatVolume)
  yield takeEvery(SET_VOICE_MUTE, updateVoiceChatMute)
  yield listenToWhetherSceneSupportsVoiceChat()

  yield put(establishingComms())
  const context: Context | undefined = yield call(connect, identity.address)
  if (context !== undefined) {
    yield put(setWorldContext(context))
  }
}

function* updateVoiceChatRecordingStatus() {
  const recording = yield select(isVoiceChatRecording)
  updateVoiceRecordingStatus(recording)
}

function* updateUserVoicePlaying(action: VoicePlayingUpdate) {
  updatePeerVoicePlaying(action.payload.userId, action.payload.playing)
}

function* updateVoiceChatVolume(action: SetVoiceVolume) {
  updateVoiceCommunicatorVolume(action.payload.volume)
}

function* updateVoiceChatMute(action: SetVoiceMute) {
  updateVoiceCommunicatorMute(action.payload.mute)
}

function* updatePlayerVoiceRecording(action: VoiceRecordingUpdate) {
  yield call(waitForRendererInstance)
  getUnityInstance().SetPlayerTalking(action.payload.recording)
}

function* changeRealm() {
  const currentRealm: ReturnType<typeof getRealm> = yield select(getRealm)
  if (!currentRealm) {
    DEBUG && logger.info(`No realm set, wait for actual DAO initialization`)
    // if not realm is set => wait for actual dao initialization
    return
  }

  const otherRealm = yield call(selectRealm)

  if (!sameRealm(currentRealm, otherRealm)) {
    logger.info(`Changing realm from ${realmToString(currentRealm)} to ${realmToString(otherRealm)}`)
    yield put(setCatalystRealm(otherRealm))
  } else {
    DEBUG && logger.info(`Realm already set ${realmToString(currentRealm)}`)
  }
}

function sameRealm(realm1: Realm, realm2: Realm) {
  return realm1.catalystName === realm2.catalystName && realm1.layer === realm2.layer
}
