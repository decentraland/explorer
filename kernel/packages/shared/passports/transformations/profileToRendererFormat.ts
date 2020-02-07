import { Profile } from '../types'
import { ProfileForRenderer } from 'decentraland-ecs/src'
import { convertToRGBObject } from './convertToRGBObject'
import { dropDeprecatedWearables } from './processServerProfile'
import { AuthIdentity } from '../../crypto/Authenticator'

export function profileToRendererFormat(profile: Profile, identity?: AuthIdentity): ProfileForRenderer {
  return {
    ...profile,
    ...(identity ? { hasConnectedWeb3: identity.hasConnectedWeb3 } : {}),
    avatar: {
      ...profile.avatar,
      wearables: profile.avatar.wearables.filter(dropDeprecatedWearables),
      eyeColor: convertToRGBObject(profile.avatar.eyeColor),
      hairColor: convertToRGBObject(profile.avatar.hairColor),
      skinColor: convertToRGBObject(profile.avatar.skinColor)
    }
  }
}
