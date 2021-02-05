import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import {
  spawnPortableExperienceScene,
  getPortableExperience,
  PortableExperienceHandle,
  PortableExperienceIdentifier
} from 'unity-interface/portableExperiencesUtils'
import { BrowserInterface } from 'unity-interface/BrowserInterface'
import { ExposableAPI } from './ExposableAPI'
import { ParcelIdentity } from './ParcelIdentity'

type SpawnPortableExperienceParameters = {
  urn: PortableExperienceIdentifier
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
    const parcelIdentity: ParcelIdentity = this.options.getAPIInstance(ParcelIdentity)
    const portableExperience: PortableExperienceHandle = await spawnPortableExperienceScene(
      spawnParams.urn,
      parcelIdentity.cid
    )

    return portableExperience
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

    const portableExperience = getPortableExperience(pid)

    if (!!portableExperience && portableExperience.parentCid == parcelIdentity.cid) {
      BrowserInterface.KillPortableExperience({ portableExperienceId: pid })
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

    BrowserInterface.KillPortableExperience({ portableExperienceId: parcelIdentity.cid })
    return true
  }
}
