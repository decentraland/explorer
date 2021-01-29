import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'

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
  contentIdentifier: ContentIdentifier
  portableExperienceId: PortableExperienceIdentifier
}

@registerAPI('PortableExperiences')
export class PortableExperiences extends ExposableAPI {
  /**
   * Starts a portable experience.
   * @param  {SpawnPortableExperienceParameters} [spawnParams] - Information to identify the PE
   *
   * Returns the handle of the portable experience.
   */
  @exposeMethod
  async spawn(spawnParams: SpawnPortableExperienceParameters): Promise<PortableExperienceHandle> {
    return {
      pid: 'test',
      identifier: spawnParams.portableExperienceId,
      parentProcess: { type: ExecutorType.QUEST_UI, identifier: 'test' }
    }
  }
}
