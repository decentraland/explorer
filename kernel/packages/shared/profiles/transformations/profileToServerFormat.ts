import { analizeColorPart, stripAlpha } from './analizeColorPart'
import { isValidBodyShape } from './isValidBodyShape'
import { Profile, Snapshots } from '../types'
import { WearableId } from 'decentraland-ecs'

export function ensureServerFormat(profile: Profile): ServerFormatProfile {
  const { avatar } = profile
  const eyes = stripAlpha(analizeColorPart(avatar, 'eyeColor', 'eyes'))
  const hair = stripAlpha(analizeColorPart(avatar, 'hairColor', 'hair'))
  const skin = stripAlpha(analizeColorPart(avatar, 'skin', 'skinColor'))
  const invalidWearables =
    !avatar.wearables ||
    !Array.isArray(avatar.wearables) ||
    !avatar.wearables.reduce((prev: boolean, next: any) => prev && typeof next === 'string', true)
  if (invalidWearables) {
    throw new Error('Invalid Wearables array! Received: ' + JSON.stringify(avatar))
  }
  if (!avatar.snapshots || !avatar.snapshots.face || !avatar.snapshots.body) {
    throw new Error('Invalid snapshot data:' + JSON.stringify(avatar.snapshots))
  }
  if (!avatar.bodyShape || !isValidBodyShape(avatar.bodyShape)) {
    throw new Error('Invalid BodyShape! Received: ' + JSON.stringify(avatar))
  }

  return {
    ...profile,
    avatar: {
      bodyShape: mapLegacyIdToUrn(avatar.bodyShape), // These mappings from legacy id are here just in case they still have the legacy id in local storage
      snapshots: avatar.snapshots,
      eyes: { color: eyes },
      hair: { color: hair },
      skin: { color: skin },
      wearables: avatar.wearables.map(mapLegacyIdToUrn)
    }
  }
}

function mapLegacyIdToUrn(wearableId: WearableId): WearableId {
  if (!wearableId.startsWith('dcl://')) {
    return wearableId
  }
  if (wearableId.startsWith('dcl://base-avatars')) {
    const name = wearableId.substring(wearableId.lastIndexOf('/') + 1)
    return `urn:decentraland:off-chain:base-avatars:${name}`
  } else {
    const [collectionName, wearableName] = wearableId.replace('dcl://', '').split('/')
    return `urn:decentraland:ethereum:collections-v1:${collectionName}:${wearableName}`
  }
}

export function buildServerMetadata(profile: Profile) {
  const newProfile = ensureServerFormat(profile)
  const metadata = { avatars: [newProfile] }
  return metadata
}

export type ServerFormatProfile = Omit<Profile, 'avatar'> & {
  avatar: ServerProfileAvatar
}

type Color3 = {
  r: number
  g: number
  b: number
}

type ServerProfileAvatar = {
  bodyShape: WearableId
  eyes: { color: Color3 }
  hair: { color: Color3 }
  skin: { color: Color3 }
  wearables: WearableId[]
  snapshots: Snapshots
}
