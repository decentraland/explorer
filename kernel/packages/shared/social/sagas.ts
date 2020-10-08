import { put, select, takeEvery } from 'redux-saga/effects'
import { getCurrentUserId } from 'shared/session/selectors'
import { getProfile } from 'shared/profiles/selectors'
import { saveProfileRequest } from 'shared/profiles/actions'
import {
  BlockPlayer,
  BLOCK_PLAYER,
  MutePlayer,
  MUTE_PLAYER,
  UnblockPlayer,
  UNBLOCK_PLAYER,
  UnmutePlayer,
  UNMUTE_PLAYER
} from './actions'

export function* socialSaga(): any {
  yield takeEvery(MUTE_PLAYER, saveMutedPlayer)
  yield takeEvery(BLOCK_PLAYER, saveBlockedPlayer)
  yield takeEvery(UNMUTE_PLAYER, saveUnmutedPlayer)
  yield takeEvery(UNBLOCK_PLAYER, saveUnblockedPlayer)
}

function* saveMutedPlayer(action: MutePlayer) {}

function* saveBlockedPlayer(action: BlockPlayer) {
  const profile = yield getCurrentProfile()

  if (profile) {
    let blocked: string[] = [action.payload.playerId]

    if (profile.blocked) {
      for (let blockedUser of profile.blocked) {
        if (blockedUser === action.payload.playerId) {
          return
        }
      }

      // Merge the existing array and any previously blocked users
      blocked = [...profile.blocked, ...blocked]
    }

    yield put(saveProfileRequest({ ...profile, blocked }))
  }
}

function* saveUnmutedPlayer(action: UnmutePlayer) {}

function* saveUnblockedPlayer(action: UnblockPlayer) {
  const profile = yield getCurrentProfile()

  if (profile) {
    const blocked = profile.blocked ? profile.blocked.filter((id) => id !== action.payload.playerId) : []
    yield put(saveProfileRequest({ ...profile, blocked }))
  }
}

function* getCurrentProfile() {
  const address = yield select(getCurrentUserId)
  const profile = yield select(getProfile, address)
  return profile
}
