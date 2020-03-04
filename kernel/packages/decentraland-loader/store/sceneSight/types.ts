import { SceneId, StringCoordinate } from '../sceneInfo/types'

export type SceneSightState = Record<SceneId, number>
export type SightMap = Record<StringCoordinate, SceneId>
export type FlexibleSightMap = Record<SceneId, number>
