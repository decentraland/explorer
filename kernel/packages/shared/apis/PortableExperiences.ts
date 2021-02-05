import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import {
  spawnPortableExperienceScene,
  getPortableExperience,
  PortableExperienceHandle,
  PortableExperienceIdentifier,
  PortableExperienceUrn,
  killPortableExperienceScene
} from 'unity-interface/portableExperiencesUtils'
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
      const portableExperienceUrn: PortableExperienceUrn = `urn:decentraland:off-chain:static-portable-experiences:${parcelIdentity.cid}`
      await killPortableExperienceScene(portableExperienceUrn)
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

    const portableExperienceUrn: PortableExperienceUrn = `urn:decentraland:off-chain:static-portable-experiences:${parcelIdentity.cid}`
    await killPortableExperienceScene(portableExperienceUrn)
    return true
  }
}
