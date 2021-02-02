declare module '@decentraland/PortableExperiences' {
  type ExecutorType = 'SCENE' | 'WEARABLE' | 'QUEST_UI'
  type Executor = {
    type: ExecutorType
    identifier: string
  }
  type PortableExperienceIdentifier = string
  type PortableExperienceHandle = {
    pid: string
    identifier: PortableExperienceIdentifier
    parentProcess: Executor
  }
  type SpawnPortableExperienceParameters = {
    urn: PortableExperienceIdentifier
  }

  export function spawn(spawnParams: SpawnPortableExperienceParameters): Promise<PortableExperienceHandle>

  export function kill(pid: PortableExperienceIdentifier): Promise<boolean>
}
