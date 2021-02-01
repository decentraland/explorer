declare module '@decentraland/PortableExperiences' {
  enum ExecutorType {
    SCENE = 'SCENE',
    WEARABLE = 'WEARABLE',
    QUEST_UI = 'QUEST_UI'
  }
  type Executor = {
    type: ExecutorType
    identifier: string
  }
  type ContentIdentifier = string
  type PortableExperienceIdentifier = string
  type PortableExperienceHandle = {
    pid: string
    identifier: PortableExperienceIdentifier
    parentProcess: Executor
  }
  type SpawnPortableExperienceParameters = {
    urn: ContentIdentifier
    portableExperienceId: PortableExperienceIdentifier
  }

  export function spawn(spawnParams: SpawnPortableExperienceParameters): Promise<PortableExperienceHandle>

  export function kill(pid: PortableExperienceIdentifier): Promise<boolean>
}
