import { ClientResponse, PlayerQuestDetails, PlayerTaskDetails, ProgressStatus } from 'dcl-quests-client'
import { takeEvery } from 'redux-saga/effects'
import { USER_AUTHENTIFIED } from 'shared/session/actions'
import { questsRequest } from './client'
import { QuestForRenderer, SectionForRenderer, TaskForRenderer } from './types'
import { unityInterface } from 'unity-interface/UnityInterface'

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

function toRendererQuest(details: PlayerQuestDetails): QuestForRenderer {
  return {
    id: details.id,
    name: details.name,
    description: details.description,
    icon: details.icon,
    thumbnail_banner: details.thumbnailBanner,
    thumbnail_entry: details.thumbnailEntry,
    sections: toRendererSections(details.tasks)
  }
}

function toRendererSections(tasks: PlayerTaskDetails[]): SectionForRenderer[] {
  const sectionsMap = tasks.reduce<Record<string, SectionForRenderer>>((currentMap, task) => {
    const sectionName = task.section ?? ''
    const section = currentMap[sectionName] ?? { id: sectionName, name: sectionName, progress: 0, tasks: [] }

    section.tasks.push(toRendererTask(task))

    currentMap[sectionName] = section
    return currentMap
  }, {})

  const sections = Object.values(sectionsMap)

  sections.forEach((it) => {
    it.progress = it.tasks.length > 0 ? it.tasks.reduce((a, b) => a + b.progress, 0) / it.tasks.length : 0
  })

  return sections
}

function toRendererTask(task: PlayerTaskDetails): TaskForRenderer {
  return {
    id: task.id,
    progress: task.progressPercentage,
    name: task.description,
    coordinates: task.coordinates,
    payload: JSON.stringify(getProgressPayload(task)),
    type: task.progressMode.type
  }
}

function getProgressPayload(task: PlayerTaskDetails) {
  const progressMode = task.progressMode
  switch (progressMode.type) {
    case 'single':
      return { isDone: task.progressStatus === ProgressStatus.COMPLETED }
    case 'numeric':
      return {
        type: 'numeric',
        start: progressMode.start,
        end: progressMode.end,
        current: task.progressSummary?.current ?? progressMode.start
      }
    case 'step-based':
      // TODO
      return {}
  }
}
