import { PlayerQuestDetails, PlayerTaskDetails, ProgressStatus } from 'dcl-quests-client'
import { QuestForRenderer, SectionForRenderer, TaskForRenderer } from './types'

export function toRendererQuest(details: PlayerQuestDetails): QuestForRenderer {
  return {
    id: details.id,
    name: details.name,
    description: details.description,
    icon: details.icon,
    thumbnail_banner: details.thumbnailBanner,
    thumbnail_entry: details.thumbnailEntry,
    status: details.progressStatus,
    sections: toRendererSections(details.tasks)
  }
}

export function toRendererSections(tasks: PlayerTaskDetails[]): SectionForRenderer[] {
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

export function toRendererTask(task: PlayerTaskDetails): TaskForRenderer {
  return {
    id: task.id,
    progress: task.progressPercentage,
    name: task.description,
    coordinates: task.coordinates,
    payload: JSON.stringify(getProgressPayload(task)),
    status: task.progressStatus,
    type: task.progressMode.type
  }
}

export function getProgressPayload(task: PlayerTaskDetails) {
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
