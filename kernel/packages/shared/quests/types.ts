export type SectionForRenderer = {
  id: string
  name: string
  progress: number
  tasks: TaskForRenderer[]
}

export type TaskForRenderer = {
  id: string
  name: string
  type: string
  progress: number
  coordinates?: string
  payload: string
}

export type QuestForRenderer = {
  id: string
  name: string
  description: string
  thumbnail_entry?: string
  thumbnail_banner?: string
  icon?: string
  sections: SectionForRenderer[]
}
