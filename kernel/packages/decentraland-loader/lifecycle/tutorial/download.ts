import { SceneDataDownloadManager, SceneMappingResponse } from "../controllers/download";
import { ILand } from "shared/types";
import { onTutorialTeleport } from "./tutorial";
import future from "fp-future";
import { ParcelLifeCycleController, ParcelSightSeeingReport } from "../controllers/parcel";
import { Vector2Component } from "atomicHelpers/landHelpers";
import { PositionLifecycleController } from "../controllers/position";

const tutorialSceneContents = require('./tutorialSceneContents.json')
const TUTORIAL_SCENE_COORDS = { x: 200, y: 200 }
const TUTORIAL_SCENE_ID = 'TutorialScene'

export class SceneDataDownloadManager_Tutorial extends SceneDataDownloadManager
{
  public async getParcelDataBySceneId(sceneId: string): Promise<ILand | null> {
    return this.getTutorialParcelDataBySceneId()
  }

  public async getParcelData(position: string): Promise<ILand | null> {
    return this.resolveTutorialScene()
  }

  async resolveTutorialScene(): Promise<ILand | null> {
    if (this.sceneIdToLandData.has(TUTORIAL_SCENE_ID)) {
      return this.sceneIdToLandData.get(TUTORIAL_SCENE_ID)!
    }
    const promised = future<ILand | null>()
    const tutorialScene = this.createTutorialILand(this.options.tutorialBaseURL)
    const contents = {
      data: [
        {
          parcel_id: tutorialScene.mappingsResponse.parcel_id,
          root_cid: tutorialScene.mappingsResponse.root_cid,
          scene_cid: ''
        }
      ]
    } as SceneMappingResponse
    this.setSceneRoots(contents)
    this.sceneIdToLandData.set(TUTORIAL_SCENE_ID, promised)
    promised.resolve(tutorialScene)
    return promised
  }

  async getTutorialParcelDataBySceneId(): Promise<ILand | null> {
    return this.sceneIdToLandData.get(TUTORIAL_SCENE_ID)!
  }
  
  private createTutorialILand(baseLocation: string): ILand {
    const coordinates = `${TUTORIAL_SCENE_COORDS.x},${TUTORIAL_SCENE_COORDS.y}`
    return {
      sceneId: TUTORIAL_SCENE_ID,
      baseUrl: baseLocation + '/loader/tutorial-scene/',
      name: 'Tutorial Scene',
      baseUrlBundles: '',
      scene: {
        name: 'Tutorial Scene',
        main: 'bin/game.js',
        scene: { parcels: this.getSceneParcelsCoords(), base: coordinates },
        communications: { commServerUrl: '' },
        spawnPoints: [
          {
            name: 'spawnPoint',
            position: {
              x: 37,
              y: 2.5,
              z: 60.5
            }
          }
        ]
      },
      mappingsResponse: {
        parcel_id: coordinates,
        contents: tutorialSceneContents,
        root_cid: TUTORIAL_SCENE_ID,
        publisher: '0x13371b17ddb77893cd19e10ffa58461396ebcc19'
      }
    }
  }
  
  private getSceneParcelsCoords() : any {
    let ret = []
    for (let i = 0; i < 6; i++) {
      for (let j = 0; j < 6; j++) {
        ret.push(`${TUTORIAL_SCENE_COORDS.x + i},${TUTORIAL_SCENE_COORDS.y + j}`)
      }
    }
    return ret
  }
}

export class ParcelLifeCycleController_Tutorial extends ParcelLifeCycleController
{
  reportCurrentPosition(position: Vector2Component): ParcelSightSeeingReport | undefined {
    if (this.currentPosition && this.currentPosition.x === position.x && this.currentPosition.y === position.y) {
      // same position, no news
      return undefined
    }

    return this.doReportCurrentPosition(position, { ...this.config, lineOfSightRadius: 0 })
  }
}

export class PositionLifecycleController_Tutorial extends PositionLifecycleController {

  public async reportCurrentPosition(position: Vector2Component, teleported: boolean) {
    await this.reportCurrentPositionTutorial(position, teleported)
  }

  private async reportCurrentPositionTutorial(position: Vector2Component, teleported: boolean) {
    const tutorialParcelCoords = this.resolveTutorialPosition(position, teleported)
    await this.doReportCurrentPosition(tutorialParcelCoords, teleported)
  }

  resolveTutorialPosition(position: Vector2Component, teleported: boolean): Vector2Component {
    if (teleported) {
      onTutorialTeleport()
    }
    return TUTORIAL_SCENE_COORDS
  }
}