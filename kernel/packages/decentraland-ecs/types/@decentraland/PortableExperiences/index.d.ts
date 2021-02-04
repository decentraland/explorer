declare module '@decentraland/PortableExperiences' {
  type PortableExperienceIdentifier = string
  type PortableExperienceHandle = {
    pid: PortableExperienceIdentifier
    cid: string
  }
  type SpawnPortableExperienceParameters = {
    urn: PortableExperienceIdentifier
  }

  export function spawn(spawnParams: SpawnPortableExperienceParameters): Promise<PortableExperienceHandle>

  export function kill(pid: PortableExperienceIdentifier): Promise<boolean>

  export function exit(): Promise<boolean>
}
