import { StoreContainer } from '../shared/store/rootTypes'
import { getExploreRealmsService, getFetchContentServer, getRealm } from '../shared/dao/selectors'
import { CurrentRealmInfoForRenderer, RealmsInfoForRenderer } from '../shared/types'
import { observeRealmChange } from '../shared/dao'
import { Realm } from '../shared/dao/types'
import { unityInterface } from './UnityInterface'
import defaultLogger from '../shared/logger'

const REPORT_INTERVAL = 2 * 60 * 1000

declare const globalThis: StoreContainer

let isReporting = false

export function startRealmsReportToRenderer() {
  if (!isReporting) {
    isReporting = true

    const realm = getRealm(globalThis.globalStore.getState())
    if (realm) {
      reportToRenderer({ current: convertCurrentRealmType(realm) })
    }

    observeRealmChange(globalThis.globalStore, (previous, current) => {
      reportToRenderer({ current: convertCurrentRealmType(current) })
    })

    fetchAndReportRealmsInfo().catch((e) => defaultLogger.log(e))

    setInterval(async () => {
      await fetchAndReportRealmsInfo()
    }, REPORT_INTERVAL)
  }
}

async function fetchAndReportRealmsInfo() {
  const url = getExploreRealmsService(globalThis.globalStore.getState())
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
  unityInterface.UpdateRealmsInfo(info)
}

function convertCurrentRealmType(realm: Realm): CurrentRealmInfoForRenderer {
  const contentServerUrl = getFetchContentServer(globalThis.globalStore.getState())
  return {
    serverName: realm.catalystName,
    layer: realm.layer,
    domain: realm.domain,
    contentServerUrl: contentServerUrl
  }
}
