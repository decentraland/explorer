import { ILand } from 'shared/types'

const tutorialSceneContents = require('./tutorialSceneContents.json')

export const TUTORIAL_SCENE_COORDS = { x: 200, y: 200 }
export const TUTORIAL_SCENE_ID = 'TutorialScene'

export const isTutorial = true

export function createTutorialILand(baseLocation: string): ILand {
  const coordinates = `${TUTORIAL_SCENE_COORDS.x},${TUTORIAL_SCENE_COORDS.y}`
  return {
    sceneId: TUTORIAL_SCENE_ID,
    baseUrl: baseLocation + '/loader/tutorial-scene/',
    name: 'Tutorial Scene',
    baseUrlBundles: '',
    scene: {
      name: 'Tutorial Scene',
      main: 'bin/game.js',
      scene: { parcels: getSceneParcelsCoords(), base: coordinates },
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

function getSceneParcelsCoords() {
  let ret = []
  for (let i = 0; i < 6; i++) {
    for (let j = 0; j < 6; j++) {
      ret.push(`${TUTORIAL_SCENE_COORDS.x + i},${TUTORIAL_SCENE_COORDS.y + j}`)
    }
  }
  return ret
}
