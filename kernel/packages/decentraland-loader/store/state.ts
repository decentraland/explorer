import { DownloadState } from './download/downloadState'
import { PositionState } from './position/positionState'
import { SightInfo } from './position/sightInfo'
import { SceneInfoState } from './sceneInfo/types'
import { SceneSightState } from './sceneSight/types'
import { SceneStatus } from './sceneSight/sceneStatus'
import { ConfigState } from './config/config'

export type RootState = {
  configuration: ConfigState
  position: PositionState
  sightInfo: SightInfo
  download: DownloadState
  sceneInfo: SceneInfoState
  sceneSight: SceneSightState
  sceneState: SceneStatus
}
