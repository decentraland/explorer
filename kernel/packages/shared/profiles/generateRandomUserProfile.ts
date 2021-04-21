import { Profile } from './types'
import { getFetchContentServer } from 'shared/dao/selectors'
import { Store } from 'redux'
import { createFakeName } from './utils/fakeName'
import { profileServerRequest } from './sagas'

declare const window: Window & { globalStore: Store }

function randomBetween(min: number, max: number) {
  return Math.floor(Math.random() * (max - min + 1) + min)
}

export async function generateRandomUserProfile(userId: string): Promise<Profile> {
  const _number = randomBetween(1, 160)

  let profile: any | undefined = undefined
  try {
    const profiles: { avatars: object[] } = await profileServerRequest(`default${_number}`)
    if (profiles.avatars.length !== 0) {
      profile = profiles.avatars[0]
    }
  } catch (e) {
    // in case something fails keep going and use backup profile
  }

  if (!profile) {
    profile = backupProfile(getFetchContentServer(window.globalStore.getState()), userId)
  }

  profile.unclaimedName = createFakeName()
  profile.hasClaimedName = false
  profile.tutorialStep = 0
  profile.version = -1 // We signal random user profiles with -1

  return profile
}

export function backupProfile(contentServerUrl: string, userId: string) {
  return {
    userId,
    email: '',
    inventory: [],
    hasClaimedName: false,
    ethAddress: 'noeth',
    tutorialStep: 0,
    name: '',
    description: '',
    avatar: {
      bodyShape: 'urn:decentraland:off-chain:base-avatars:BaseFemale',
      skin: {
        color: {
          r: 0.4901960790157318,
          g: 0.364705890417099,
          b: 0.27843138575553894
        }
      },
      hair: {
        color: {
          r: 0.5960784554481506,
          g: 0.37254902720451355,
          b: 0.21568627655506134
        }
      },
      eyes: {
        color: {
          r: 0.37254902720451355,
          g: 0.2235294133424759,
          b: 0.19607843458652496
        }
      },
      wearables: [
        'urn:decentraland:off-chain:base-avatars:f_sweater',
        'urn:decentraland:off-chain:base-avatars:f_jeans',
        'urn:decentraland:off-chain:base-avatars:bun_shoes',
        'urn:decentraland:off-chain:base-avatars:standard_hair',
        'urn:decentraland:off-chain:base-avatars:f_eyes_00',
        'urn:decentraland:off-chain:base-avatars:f_eyebrows_00',
        'urn:decentraland:off-chain:base-avatars:f_mouth_00'
      ],
      version: -1,
      snapshots: {
        face: `${contentServerUrl}/contents/QmZbyGxDnZ4PaMVX7kpA2NuGTrmnpwTJ8heKKTSCk4GRJL`,
        body: `${contentServerUrl}/contents/QmaQvcBWg57Eqf5E9R3Ts1ttPKKLhKueqdyhshaLS1tu2g`
      }
    }
  }
}
