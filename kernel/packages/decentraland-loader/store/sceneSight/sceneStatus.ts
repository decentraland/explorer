import { SceneId } from '../sceneInfo/types'

export type SceneLoadState = 'Loading' | 'Started' | 'Unloaded'

export const LOADING = 'Loading'
export const STARTED = 'Started'
export const UNLOADED = 'Unloaded'

export type SceneStatus = Record<SceneId, SceneLoadState>
