import { SceneContentServerEntity } from '../sceneInfo/types'
import { getEntity } from './getEntity'
import { contents } from './tutorialSceneDescription'

const TUTORIAL_SCENE_COORDS = { x: 200, y: 200 }
const TUTORIAL_SCENE_ID = 'TutorialScene'
const TUTORIAL_SCENE_STRING_COORDS = `${TUTORIAL_SCENE_COORDS.x},${TUTORIAL_SCENE_COORDS.y}`

let cached: {
  tutorial?: SceneContentServerEntity
} = {
  tutorial: undefined
}

export function getTutorialEntity(): SceneContentServerEntity {
  if (!cached.tutorial) {
    cached.tutorial = getEntity(
      TUTORIAL_SCENE_ID,
      buildTutorialCoords(),
      createTutorialScene(TUTORIAL_SCENE_STRING_COORDS),
      contents
    )
  }
  return cached.tutorial
}

export function createTutorialScene(baseLocation: string) {
  const coordinate = `${TUTORIAL_SCENE_COORDS.x},${TUTORIAL_SCENE_COORDS.y}`
  return {
    sceneId: TUTORIAL_SCENE_ID,
    baseUrl: baseLocation + '/loader/tutorial-scene/',
    name: 'Tutorial Scene',
    baseUrlBundles: '',
    scene: {
      name: 'Tutorial Scene',
      main: 'bin/game.js',
      scene: { parcels: buildTutorialCoords(), base: coordinate },
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
      parcel_id: coordinate,
      contents: contents,
      root_cid: TUTORIAL_SCENE_ID,
      publisher: '0x13371b17ddb77893cd19e10ffa58461396ebcc19'
    }
  }
}

function buildTutorialCoords() {
  let ret = []
  for (let i = 0; i < 6; i++) {
    for (let j = 0; j < 6; j++) {
      ret.push(`${TUTORIAL_SCENE_COORDS.x + i},${TUTORIAL_SCENE_COORDS.y + j}`)
    }
  }
  return ret
}
