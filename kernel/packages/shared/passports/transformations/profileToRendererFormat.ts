import { Profile } from '../types'
import { ProfileForRenderer } from 'decentraland-ecs/src'
import { convertToRGBObject } from './convertToRGBObject'
import { dropDeprecatedWearables } from './processServerProfile'
import { ExplorerIdentity } from 'shared'

export function profileToRendererFormat(profile: Profile, identity?: ExplorerIdentity): ProfileForRenderer {
  const { snapshots, ...rendererAvatar } = profile.avatar
  return {
    ...profile,
    snapshots,
    hasConnectedWeb3: identity ? identity.hasConnectedWeb3 : false,
    avatar: {
      ...rendererAvatar,
      wearables: profile.avatar.wearables.filter(dropDeprecatedWearables),
      eyeColor: convertToRGBObject(profile.avatar.eyeColor),
      hairColor: convertToRGBObject(profile.avatar.hairColor),
      skinColor: convertToRGBObject(profile.avatar.skinColor)
    }
  }
}
