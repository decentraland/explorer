declare module '@decentraland/PortableExperiences' {
  type PortableExperienceUrn = string
  type PortableExperienceHandle = {
    pid: PortableExperienceUrn
    parentCid: string
  }
  type SpawnPortableExperienceParameters = {
    urn: PortableExperienceUrn
  }

  export function spawn(spawnParams: SpawnPortableExperienceParameters): Promise<PortableExperienceHandle>

  export function kill(pid: PortableExperienceUrn): Promise<boolean>

  export function exit(): Promise<boolean>
}
