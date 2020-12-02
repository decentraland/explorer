import { StoreContainer } from '../shared/store/rootTypes'
import { getExploreRealmsService, getRealm } from '../shared/dao/selectors'
import { RealmsInfoForRenderer } from '../shared/types'
import { observeRealmChange } from 'shared/dao'
import { Realm } from 'shared/dao/types'
import { unityInterface } from './UnityInterface'

const REPORT_INTERVAL = 2 * 60 * 1000

declare const globalThis: StoreContainer

let isReporting = false

export function startRealmsReportToRenderer() {
  if (!isReporting) {
    isReporting = true

    const realm = getRealm(globalThis.globalStore.getState())
    if (realm) {
      reportToRenderer({ current: convertRealmType(realm) })
    }

    observeRealmChange(globalThis.globalStore, (previous, current) => {
      reportToRenderer({ current: convertRealmType(current) })
    })

    fetchAndReportRealmsInfo()

    reportInterval()
  }
}

function reportInterval() {
  setInterval(() => {
    fetchAndReportRealmsInfo()
    reportInterval()
  }, REPORT_INTERVAL)
}

function fetchAndReportRealmsInfo() {
  const url = getExploreRealmsService(globalThis.globalStore.getState())
  fetch(url)
    .then((response) => {
      if (response.ok) {
        response
          .json()
          .then((value) => reportToRenderer({ realms: value }))
          .catch()
      }
    })
    .catch()
}

function reportToRenderer(info: Partial<RealmsInfoForRenderer>) {
  unityInterface.UpdateRealmsInfo(info)
}

function convertRealmType(realm: Realm): { serverName: string; layer: string } {
  return { serverName: realm.catalystName, layer: realm.layer }
}
