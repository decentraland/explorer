import { store } from "../state/redux"
import { getAnalyticsContext } from "../state/selectors"

export function getTLD() {
  if (globalThis.location.search && globalThis.location.search.includes("ENV=")) {
    return globalThis.location.search.match(/ENV=(\w+)/)![1]
  }
  return globalThis.location.hostname.match(/(\w+)$/)![0]
}

let analyticsDisabled = false

enum AnalyticsAccount {
  PRD = "1plAT9a2wOOgbPCrTaU8rgGUMzgUTJtU",
  DEV = "a4h4BC4dL1v7FhIQKKuPHEdZIiNRDVhc",
}

// TODO fill with segment keys and integrate identity server
export function configureSegment() {
  const TLD = getTLD()
  switch (TLD) {
    case "org":
      if (
        globalThis.location.host === "play.decentraland.org" ||
        globalThis.location.host === "explorer.decentraland.org"
      ) {
        return initialize(AnalyticsAccount.PRD)
      }
      return initialize(AnalyticsAccount.DEV)
    case "today":
      return initialize(AnalyticsAccount.DEV)
    case "zone":
      return initialize(AnalyticsAccount.DEV)
    default:
      return initialize(AnalyticsAccount.DEV)
  }
}

// once this function is called, no more errors will be tracked neither reported to rollbar
export function disableAnalytics(){
  analyticsDisabled = true
  if ((window as any).Rollbar) {
    (window as any).Rollbar.configure({ enabled: false })
  }
}

export function identifyUser(userId: string) {
  if (window.analytics) {
    window.analytics.identify(userId, getAnalyticsContext(store.getState()))
  }
}

async function initialize(segmentKey: string): Promise<void> {
  if (window.analytics.load) {
    // loading client for the first time
    window.analytics.load(segmentKey)
    window.analytics.page()
    window.analytics.ready(() => {
      window.analytics.timeout(1000)
    })
  }
}

export function trackEvent(eventName: string, eventData: Record<string, any>) {
  if (!window.analytics || analyticsDisabled) {
    return
  }

  const data = { ...eventData, ...getAnalyticsContext(store.getState()) }

  window.analytics.track(eventName, data)
}
