import { action } from 'typesafe-actions'

export const SCENE_LOAD = '[SCENE MANAGER] Loading scene'
export const SCENE_START = '[SCENE MANAGER] Started scene'
export const SCENE_FAIL = '[SCENE MANAGER] Failed scene'
export const PENDING_SCENES = '[SCENE MANAGER] Pending count'

export const signalSceneLoad = (sceneId: string) => action(SCENE_LOAD, sceneId)
export const signalSceneStart = (sceneId: string) => action(SCENE_START, sceneId)
export const signalSceneFail = (sceneId: string) => action(SCENE_FAIL, sceneId)
export const informPendingScenes = (pendingScenes: number, totalScenes: number) =>
  action(PENDING_SCENES, { pendingScenes, totalScenes })

export type SceneLoad = ReturnType<typeof signalSceneLoad>
export type SceneStart = ReturnType<typeof signalSceneStart>
export type SceneFail = ReturnType<typeof signalSceneFail>
export type InformPendingScenes = ReturnType<typeof informPendingScenes>

export const UPDATE_STATUS_MESSAGE = '[RENDERER] Update loading message'
export const updateStatusMessage = (message: string, loadPercentage: number) =>
  action(UPDATE_STATUS_MESSAGE, { message, loadPercentage })
export type UpdateStatusMessage = ReturnType<typeof updateStatusMessage>
