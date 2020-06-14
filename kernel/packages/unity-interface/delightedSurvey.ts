import { getUserProfile } from 'shared/comms/peers'
import { globalDCL } from 'shared/globalDCL'
import { defaultLogger } from 'shared/logger'
import { Profile } from 'shared/profiles/types'

export function delightedSurvey() {
  // tslint:disable-next-line:strict-type-predicates
  if (typeof globalDCL === 'undefined' || typeof globalDCL !== 'object') {
    return
  }
  const { analytics, delighted } = globalDCL
  if (!analytics || !delighted) {
    return
  }
  const profile = getUserProfile().profile as Profile | null
  if (!globalDCL.isTheFirstLoading && profile) {
    const payload = {
      email: profile.email || profile.ethAddress + '@dcl.gg',
      name: profile.name || 'Guest',
      properties: {
        ethAddress: profile.ethAddress,
        anonymous_id: analytics && analytics.user ? analytics.user().anonymousId() : null
      }
    }

    try {
      delighted.survey(payload)
    } catch (error) {
      defaultLogger.error('Delighted error: ' + error.message, error)
    }
  }
}
