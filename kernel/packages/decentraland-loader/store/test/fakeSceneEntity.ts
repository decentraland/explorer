import { SceneContentServerEntity, StringCoordinate } from '../sceneInfo/types'
import { splitCoordinate } from '../utils/splitCoordinate'

export function fakeScene(from: StringCoordinate, to: StringCoordinate): SceneContentServerEntity {
  const [x, y] = splitCoordinate(from)
  const [x2, y2] = splitCoordinate(to)
  const parcels = []
  for (let i = x; i <= x2; i++) {
    for (let j = y; j <= y2; j++) {
      parcels.push(`${i},${j}`)
    }
  }
  return {
    id: 'Scene-' + from,
    content: [{ file: 'filename', hash: 'long string' }],
    metadata: {
      baseUrl: 'baseurl',
      baseUrlBundles: 'basebundles',
      mappingsResponse: {
        contents: [{ file: 'filename', hash: 'long string' }],
        parcel_id: 'Scene-' + from,
        publisher: 'published',
        root_cid: 'Scene-' + from
      },
      name: 'Scene',
      sceneId: 'Scene-' + from,
      scene: {
        display: { title: 'Scene' },
        scene: {
          base: from,
          parcels
        },
        spawnPoints: [
          {
            cameraTarget: { x: 0, y: 0, z: 0 },
            position: { x: [0, 1], y: 0, z: 0 }
          }
        ]
      }
    },
    pointers: parcels,
    type: 'scene'
  }
}
