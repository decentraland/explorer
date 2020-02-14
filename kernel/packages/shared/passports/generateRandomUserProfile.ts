import { Profile } from './types'
import { getFetchProfileServer, getFetchContentServer } from 'shared/dao/selectors'
import { Store } from 'redux'

declare const window: Window & { store: Store }

function randomBetween(min: number, max: number) {
  return Math.floor(Math.random() * (max - min + 1) + min)
}

export async function generateRandomUserProfile(userId: string): Promise<Profile> {
  const _number = randomBetween(1, 160)
  const profileUrl = `${getFetchProfileServer(window.store.getState())}/default${_number}`

  let profile: any | undefined = undefined
  try {
    const response = await fetch(profileUrl)

    if (response.ok) {
      const profiles: { avatars: object[] } = await response.json()
      if (profiles.avatars.length !== 0) {
        profile = profiles.avatars[0]
      }
    }
  } catch (e) {
    // in case something fails keep going and use backup profile
  }

  if (!profile) {
    profile = backupProfile(getFetchContentServer(window.store.getState()))
  }

  profile.name = 'Guest-' + userId.substr(2, 6)

  return profile
}

function backupProfile(contentServerUrl: string) {
  return {
    name: '',
    description: '',
    avatar: {
      bodyShape: 'dcl://base-avatars/BaseFemale',
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
        'dcl://base-avatars/f_sweater',
        'dcl://base-avatars/f_jeans',
        'dcl://base-avatars/bun_shoes',
        'dcl://base-avatars/standard_hair',
        'dcl://base-avatars/f_eyes_00',
        'dcl://base-avatars/f_eyebrows_00',
        'dcl://base-avatars/f_mouth_00'
      ],
      version: 0,
      snapshots: {
        face: `${contentServerUrl}/contents/QmZbyGxDnZ4PaMVX7kpA2NuGTrmnpwTJ8heKKTSCk4GRJL`,
        body: `${contentServerUrl}/contents/QmaQvcBWg57Eqf5E9R3Ts1ttPKKLhKueqdyhshaLS1tu2g`
      }
    }
  }
}
