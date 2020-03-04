import { list } from './emptyParcelsList'
import { getEntity } from './getEntity'
import { ConfigState } from '../config/config'
import { splitCoordinate } from '../utils/splitCoordinate'

type EmptyName = keyof typeof list
const emptySceneNames: EmptyName[] = Object.keys(list) as EmptyName[]

function fakeEmptyId(position: string) {
  return ('Qm' + position + 'm').padEnd(46, '0')
}

export function getEmptyEntity(coordinates: string, config: ConfigState) {
  const id = fakeEmptyId(coordinates)
  const [x, y] = splitCoordinate(coordinates)
  const sceneName = emptySceneNames[(x + y * 43) % emptySceneNames.length]
  const land = emptySceneILand(id, coordinates, sceneName, config)
  return getEntity(id, [coordinates], land, list[sceneName])
}

export function emptySceneILand(sceneId: string, coordinates: string, sceneName: EmptyName, config: ConfigState) {
  return {
    sceneId: sceneId,
    baseUrl: globalThis.location.origin + '/loader/empty-scenes/contents/',
    baseUrlBundles: config.contentServerBundles + '/',
    name: 'Empty parcel',
    scene: {
      display: { title: 'Empty parcel' },
      owner: '',
      contact: {},
      name: 'Empty parcel',
      main: `bin/game.js`,
      tags: [],
      scene: { parcels: [coordinates], base: coordinates },
      policy: {},
      communications: { commServerUrl: '' }
    },
    mappingsResponse: {
      parcel_id: coordinates,
      contents: list[sceneName],
      root_cid: sceneId,
      publisher: '0x13371b17ddb77893cd19e10ffa58461396ebcc19'
    }
  }
}
