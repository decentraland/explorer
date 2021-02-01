import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { spawnPortableExperienceScene, killPortableExperienceScene } from 'unity-interface/dcl'
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
    await spawnPortableExperienceScene(spawnParams.portableExperienceId)
    // TODO: Fill correctly the result
    return {
      pid: 'test',
      identifier: spawnParams.portableExperienceId,
      parentProcess: { type: ExecutorType.QUEST_UI, identifier: 'test' }
    }
  }

  /**
   * Stops a portable experience. Only the executor that spawned the portable experience has permission to kill it.
   * @param {Executor} [executor] - The executor that will stop the PE
   * @param  {string} [pid] - The portable experience process id
   *
   * Returns true if was able to kill the portable experience, false if not.
   */
  @exposeMethod
  async kill(pid: PortableExperienceIdentifier): Promise<boolean> {
    killPortableExperienceScene(pid)
    // TODO: Check if we want to return a boolean
    return true
  }
}
