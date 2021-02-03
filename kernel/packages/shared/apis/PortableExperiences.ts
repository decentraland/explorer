import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { spawnPortableExperienceScene, killPortableExperienceScene } from 'unity-interface/portableExperiencesUtils'
import { ExposableAPI } from './ExposableAPI'
import { ParcelIdentity } from './ParcelIdentity'

enum ExecutorType {
  SCENE = 'SCENE',
  WEARABLE = 'WEARABLE',
  QUEST_UI = 'QUEST_UI'
}
type Executor = {
  type: ExecutorType
  identifier: string
}
type PortableExperienceIdentifier = string
type PortableExperienceHandle = {
  pid: string
  parentProcess: Executor
}
type SpawnPortableExperienceParameters = {
  urn: PortableExperienceIdentifier
}

@registerAPI('PortableExperiences')
export class PortableExperiences extends ExposableAPI {
  private currentPortableExperiences: Map<string, Executor> = new Map()

  /**
   * Starts a portable experience.
   * @param  {SpawnPortableExperienceParameters} [spawnParams] - Information to identify the PE
   *
   * Returns the handle of the portable experience.
   */
  @exposeMethod
  async spawn(spawnParams: SpawnPortableExperienceParameters): Promise<PortableExperienceHandle> {
    const sceneId: string = await spawnPortableExperienceScene(spawnParams.urn)
    const parcelIdentity: ParcelIdentity = this.options.getAPIInstance(ParcelIdentity)
    const currentExecutor: Executor = { type: ExecutorType.SCENE, identifier: parcelIdentity.cid }
    this.currentPortableExperiences.set(sceneId, currentExecutor)

    return {
      pid: sceneId,
      parentProcess: currentExecutor
    }
  }

  /**
   * Stops a portable experience. Only the executor that spawned the portable experience has permission to kill it.
   * @param  {string} [pid] - The portable experience process id
   *
   * Returns true if was able to kill the portable experience, false if not.
   */
  @exposeMethod
  async kill(pid: string): Promise<boolean> {
    const parcelIdentity: ParcelIdentity = this.options.getAPIInstance(ParcelIdentity)
    const currentExecutor: Executor = { type: ExecutorType.SCENE, identifier: parcelIdentity.cid }
    if (
      this.currentPortableExperiences.has(pid) &&
      JSON.stringify(this.currentPortableExperiences.get(pid)) === JSON.stringify(currentExecutor)
    ) {
      killPortableExperienceScene(pid)
      this.currentPortableExperiences.delete(pid)
      return true
    }
    return false
  }

  /**
   * Stops a portable experience from the current running portable scene.
   *
   * Returns true if was able to kill the portable experience, false if not.
   */
  @exposeMethod
  async exit(): Promise<boolean> {
    const parcelIdentity: ParcelIdentity = this.options.getAPIInstance(ParcelIdentity)
    const executorCid = parcelIdentity.cid

    if (this.currentPortableExperiences.has(executorCid)) {
      killPortableExperienceScene(executorCid)
      this.currentPortableExperiences.delete(executorCid)
      return true
    }
    return false
  }
}