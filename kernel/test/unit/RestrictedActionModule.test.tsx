import * as sinon from 'sinon'
import { Vector3 } from 'decentraland-ecs/src'
import { unityInterface } from '../../packages/unity-interface/UnityInterface'
import defaultLogger from '../../packages/shared/logger'
import { RestrictedActionModule } from '../../packages/shared/apis/RestrictedActionModule'

describe('RestrictedActionModule tests', () => {
  describe('MovePlayerTo tests', () => {
    afterEach(() => sinon.restore())
    const scene = {
      land: {
        sceneJsonData: {
          display: { title: 'interactive-text', favicon: 'favicon_asset' },
          contact: { name: 'Ezequiel', email: 'ezequiel@decentraland.org' },
          owner: 'decentraland',
          scene: { parcels: ['0,101'], base: '0,101' },
          communications: { type: 'webrtc', signalling: 'https://signalling-01.decentraland.org' },
          policy: { contentRating: 'E', fly: true, voiceEnabled: true, blacklist: [] },
          main: 'game.js',
          tags: [],
          requiredPermissions: ['ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE'],
          spawnPoints: [
            { name: 'spawn1', default: true, position: { x: 0, y: 0, z: 0 }, cameraTarget: { x: 8, y: 1, z: 8 } }
          ]
        }
      }
    }
    const options = {
      apiName: '',
      system: null,
      expose: sinon.stub(),
      notify: sinon.stub(),
      on: sinon.stub(),
      getAPIInstance(name): any {}
    }

    it('should move the player', async () => {
      sinon.mock(options).expects('getAPIInstance').withArgs().once().returns(scene)

      sinon
        .mock(unityInterface)
        .expects('Teleport')
        .once()
        .withExactArgs({ position: { x: 8, y: 0, z: 1624 }, cameraTarget: undefined })

      const module = new RestrictedActionModule(options)

      await module.movePlayerTo(new Vector3(8, 0, 8))
      sinon.verify()
    })

    it('should fail when position is outside scene', async () => {
      sinon.mock(options).expects('getAPIInstance').withArgs().once().returns(scene)
      sinon
        .mock(defaultLogger)
        .expects('error')
        .once()
        .withExactArgs('Error: Position is out of scene', { x: 21, y: 0, z: 1648 })

      sinon.mock(unityInterface).expects('Teleport').never()

      const module = new RestrictedActionModule(options)

      await module.movePlayerTo(new Vector3(21, 0, 32))
      sinon.verify()
    })

    it('should fail when scene does not have permissions', async () => {
      // remove permissions
      scene.land.sceneJsonData.requiredPermissions = []

      sinon.mock(options).expects('getAPIInstance').withArgs().once().returns(scene)
      sinon.mock(unityInterface).expects('Teleport').never()
      sinon
        .mock(defaultLogger)
        .expects('error')
        .once()
        .withExactArgs('Permission "ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE" is required')

      const module = new RestrictedActionModule(options)

      await module.movePlayerTo(new Vector3(8, 0, 8))
      sinon.verify()
    })
  })
})
