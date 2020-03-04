export type ConfigState = {
  contentServer: string
  contentServerBundles: string
  lineOfSightRadius: number
  secureRadius: number
  emptyScenes: boolean
  tutorialBaseURL: string
  tutorialSceneEnabled: boolean
}

export function updateConfig(_: ConfigState, newConfig: ConfigState) {
  return newConfig
}
