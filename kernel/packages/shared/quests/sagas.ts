import { ClientResponse, PlayerQuestDetails } from 'dcl-quests-client'
import { call, delay, put, select, takeEvery } from 'redux-saga/effects'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { questsInitialized, questsUpdated, QUESTS_INITIALIZED, QUESTS_UPDATED } from './actions'
import { questsRequest } from './client'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { toRendererQuest } from '@dcl/ecs-quests/@dcl/mappings'
import { getPreviousQuests, getQuests } from './selectors'
import { deepEqual } from 'atomicHelpers/deepEqual'
import { isFeatureEnabled } from '../meta/selectors'
import { FeatureFlags } from '../meta/types'
import { waitForRendererInstance } from 'shared/renderer/sagas'
import { waitForMetaConfigurationInitialization } from 'shared/meta/sagas'

const QUESTS_REFRESH_INTERVAL = 30000

export function* questsSaga(): any {
  yield takeEvery(USER_AUTHENTIFIED, initializeQuests)
  yield takeEvery(QUESTS_INITIALIZED, initUpdateQuestsInterval)
}

function* areQuestsEnabled() {
  yield call(waitForMetaConfigurationInitialization)
  const ret: boolean = yield select(isFeatureEnabled, FeatureFlags.QUESTS, false)
  return ret
}

function* initUpdateQuestsInterval() {
  yield takeEvery(QUESTS_UPDATED, updateQuestsLogData)

  if (yield call(areQuestsEnabled)) {
    while (true) {
      yield delay(QUESTS_REFRESH_INTERVAL)
      yield updateQuests()
    }
  }
}

function* initializeQuests(): any {
  if (yield call(areQuestsEnabled)) {
    const questsResponse: ClientResponse<PlayerQuestDetails[]> = yield questsRequest((c) => c.getQuests())
    if (questsResponse.ok) {
      yield call(waitForRendererInstance)
      initQuestsLogData(questsResponse.body)
      yield put(questsInitialized(questsResponse.body))
    } else {
      yield delay(QUESTS_REFRESH_INTERVAL)
      yield initializeQuests()
    }
  }
}

function* updateQuests() {
  const questsResponse: ClientResponse<PlayerQuestDetails[]> = yield questsRequest((c) => c.getQuests())
  if (questsResponse.ok) {
    yield put(questsUpdated(questsResponse.body))
  }
}

function* updateQuestsLogData() {
  const quests: PlayerQuestDetails[] = yield select(getQuests)
  const previousQuests: PlayerQuestDetails[] | undefined = yield select(getPreviousQuests)

  function hasChanged(quest: PlayerQuestDetails) {
    const previousQuest = previousQuests?.find((it) => it.id === quest.id)
    return !previousQuest || !deepEqual(previousQuest, quest)
  }

  yield call(waitForRendererInstance)

  quests.forEach((it) => {
    if (hasChanged(it)) {
      getUnityInstance().UpdateQuestProgress(toRendererQuest(it))
    }
  })
}

function initQuestsLogData(quests: PlayerQuestDetails[]) {
  const rendererQuests = quests.map((it) => toRendererQuest(it))

  getUnityInstance().InitQuestsInfo(rendererQuests)
}
