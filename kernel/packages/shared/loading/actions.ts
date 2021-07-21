import { action } from 'typesafe-actions'

export const SCENE_LOAD = 'Loading scene'
export const SCENE_START = 'Started scene'
export const SCENE_FAIL = 'Failed scene'
export const PENDING_SCENES = '[SCENE MANAGER] Pending count'

export const signalSceneLoad = (sceneId: string) => action(SCENE_LOAD, sceneId)
export const signalSceneStart = (sceneId: string) => action(SCENE_START, sceneId)
export const signalSceneFail = (sceneId: string) => action(SCENE_FAIL, sceneId)
export const signalPendingScenes = (pendingScenes: number, totalScenes: number) =>
  action(PENDING_SCENES, { pendingScenes, totalScenes })

export type SceneLoad = ReturnType<typeof signalSceneLoad>
export type SceneStart = ReturnType<typeof signalSceneStart>
export type SceneFail = ReturnType<typeof signalSceneFail>
export type SignalPendingScenes = ReturnType<typeof signalPendingScenes>

export const UPDATE_STATUS_MESSAGE = 'Update status message'
export const updateStatusMessage = (message: string, loadPercentage: number) =>
  action(UPDATE_STATUS_MESSAGE, { message, loadPercentage })
export type UpdateStatusMessage = ReturnType<typeof updateStatusMessage>
