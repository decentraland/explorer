import { StoreType } from "./redux"

export function getAnalyticsContext(state: StoreType) {
  return {
    sessionId: state.featureFlags.sessionId,
    version: (globalThis as any)["VERSION"],
    rendererVersion: state.renderer.version,
    kernelVersion: state.kernel.kernel?.version,
    userId: state.session.kernelState?.identity?.address,
  }
}
