declare module '@decentraland/QuestsController' {
  export enum ProgressStatus {
    NOT_STARTED = 'not_started',
    ON_GOING = 'on_going',
    COMPLETED = 'completed',
    FAILED = 'failed'
  }

  export type PlayerQuestDetails = {
    id: string
    name: string
    description: string
    thumbnail?: string
    active: boolean
    progressStatus: ProgressStatus
    tasks: PlayerTaskDetails[]
    requirements: any[]
  }

  export type ProgressSummary = {
    current: number
    target: number
    start: number
    unit: string
  }

  export type CountProgressData = {
    type: 'count'
    amount: number
  } & BaseProgress

  export type ArbitraryProgressData = {
    type: 'arbitrary'
    current: number
  } & BaseProgress

  export type SingleProgressData = {
    type: 'single'
    status: ProgressStatus
  } & BaseProgress

  export type BaseProgress = {
    challenge?: any
  }

  export type StepBasedProgressData = {
    type: 'step-based'
    stepStatus: ProgressStatus
    stepId: string
  } & BaseProgress

  export type PlayerTaskProgress = {
    id: string
    step?: any
    progressData: ProgressData
    status: ProgressStatus
    date: Date
  }

  export type PlayerTaskDetails = {
    id: string
    description: string
    progressMode: any
    coordinates?: string
    progressStatus: ProgressStatus
    required: boolean
    section?: string
    progressSummary?: ProgressSummary
    lastProgress?: PlayerTaskProgress
    steps: {
      id: string
    }[]
    requirements: any[]
  }

  export type ProgressData = SingleProgressData | CountProgressData | ArbitraryProgressData | StepBasedProgressData

  export type ErrorResponse = {
    status: string
    message: string
    errorData?: any
  }

  export type FailedQuestsResponse = {
    ok: false
    body?: ErrorResponse
  }

  export type OkQuestsResponse<T> = {
    ok: true
    body: T
  }

  export type QuestsResponse<T> = OkQuestsResponse<T> | FailedQuestsResponse

  export function getQuests(): Promise<QuestsResponse<PlayerQuestDetails[]>>

  export function getQuestDetails(questId: string): Promise<QuestsResponse<PlayerQuestDetails>>

  export function startQuest(questId: string): Promise<QuestsResponse<PlayerQuestDetails>>

  export function makeProgress(
    questId: string,
    taskId: string,
    progress: ProgressData
  ): Promise<QuestsResponse<PlayerQuestDetails>>
}
