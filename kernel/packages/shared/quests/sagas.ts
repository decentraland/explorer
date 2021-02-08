import { ClientResponse, PlayerQuestDetails } from 'dcl-quests-client'
import { takeEvery } from 'redux-saga/effects'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { questsRequest } from './client'
import { unityInterface } from 'unity-interface/UnityInterface'
import { toRendererQuest } from './mappings'

export function* questsSaga(): any {
  yield takeEvery(USER_AUTHENTIFIED, fetchQuests)
}

function* fetchQuests() {
  const questsResponse: ClientResponse<PlayerQuestDetails[]> = yield questsRequest((c) => c.getQuests())
  if (questsResponse.ok) {
    updateQuestsLogData(questsResponse.body)
  }
}

function updateQuestsLogData(quests: PlayerQuestDetails[]) {
  const rendererQuests = quests.map((it) => toRendererQuest(it))

  unityInterface.InitQuestsInfo(rendererQuests)
}
