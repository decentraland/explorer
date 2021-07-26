import { getExploreRealmsService, getFetchContentServer, getRealm } from '../shared/dao/selectors'
import { CurrentRealmInfoForRenderer, RealmsInfoForRenderer } from '../shared/types'
import { observeRealmChange } from '../shared/dao'
import { Realm } from '../shared/dao/types'
import { getUnityInstance } from './IUnityInterface'
import defaultLogger from '../shared/logger'
import { store } from 'shared/store/isolatedStore'

const REPORT_INTERVAL = 2 * 60 * 1000

let isReporting = false

export function startRealmsReportToRenderer() {
  if (!isReporting) {
    isReporting = true

    const realm = getRealm(store.getState())
    if (realm) {
      reportToRenderer({ current: convertCurrentRealmType(realm) })
    }

    observeRealmChange(store, (previous, current) => {
      reportToRenderer({ current: convertCurrentRealmType(current) })
    })

    fetchAndReportRealmsInfo().catch((e) => defaultLogger.log(e))

    setInterval(async () => {
      await fetchAndReportRealmsInfo()
    }, REPORT_INTERVAL)
  }
}

async function fetchAndReportRealmsInfo() {
  const url = getExploreRealmsService(store.getState())
  try {
    const response = await fetch(url)
    if (response.ok) {
      const value = await response.json()
      reportToRenderer({ realms: value })
    }
  } catch (e) {
    defaultLogger.log(e)
  }
}

function reportToRenderer(info: Partial<RealmsInfoForRenderer>) {
  getUnityInstance().UpdateRealmsInfo(info)
}

function convertCurrentRealmType(realm: Realm): CurrentRealmInfoForRenderer {
  const contentServerUrl = getFetchContentServer(store.getState())
  return {
    serverName: realm.catalystName,
    layer: realm.layer ?? '',
    domain: realm.domain,
    contentServerUrl: contentServerUrl
  }
}
