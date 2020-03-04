import { expect } from 'chai'
import { describe, it } from 'mocha'
import {
  bootstrapAt,
  downloadEnqueuedParcels,
  enqueueUnknownParcels,
  processParcelSightChange,
  processSceneSightChange,
  processUserTeleport,
  resolveSpawnPosition,
  storeResolvedSceneEntity
} from './actions'
import { rootReducer } from './reducer'
import { generateSightMap } from './sceneSight/sceneSight'
import { shouldSelectSpawnTarget } from './selectors/canSpawn'
import { getCurrentSceneEntity } from './selectors/getCurrentScene'
import { pickWorldSpawnpoint } from './selectors/getSpawnPosition'
import { fakeScene } from './test/fakeSceneEntity'

describe('loader reducer', () => {
  const initialState = rootReducer()
  const enqueue = rootReducer(initialState, enqueueUnknownParcels())
  const scene = fakeScene('-3,-3', '3,3')
  const resolved = rootReducer(initialState, storeResolvedSceneEntity(scene))
  const resolvedAndInSight = rootReducer(resolved, processParcelSightChange())
  const downloading = rootReducer(enqueue, downloadEnqueuedParcels())
  const currentScene = getCurrentSceneEntity(resolved)
  const spawnTarget = pickWorldSpawnpoint(currentScene.metadata.scene)
  const afterSpawn = rootReducer(
    resolvedAndInSight,
    resolveSpawnPosition(spawnTarget.position, spawnTarget.cameraTarget)
  )

  describe('bootstrap', () => {
    it('works correctly', () => {
      expect(initialState.sightInfo.inSight).to.have.lengthOf(29)
      expect(initialState).to.have.keys('sceneSight', 'sightInfo', 'download', 'position', 'sceneInfo', 'sceneState')
    })
    it('works with an initial coordinate', () => {
      const at55 = rootReducer(undefined, bootstrapAt('5,5'))
      expect(at55.sightInfo.inSight).to.have.lengthOf(29)
      expect(at55.position.isRendering).to.eq(false)
      expect(at55.position.isTeleporting).to.eq(false)
      expect(at55.position.targetPosition).to.eq('5,5')
      expect(at55.sightInfo.inSightDict['5,5']).to.eq(true)
    })
  })

  describe('enqueue unknown parcels', () => {
    it('queues all the parcels that are in sight but unknown', () => {
      const afterEnqueue = rootReducer(initialState, enqueueUnknownParcels())
      expect(afterEnqueue.download.queued).to.contain('0,0')
    })
  })

  describe('download parcels', () => {
    it('marks parcels as being downloaded', () => {
      expect(Object.keys(downloading.download.pendingDownloads)).to.have.length(29)
      expect(downloading.download.queued).to.have.length(0)
    })
    it('resolving an entity produces a known parcel', () => {
      const downloadedResolved = rootReducer(downloading, storeResolvedSceneEntity(fakeScene('-3,-3', '3,3')))

      expect(Object.keys(downloadedResolved.download.pendingDownloads)).to.have.length(0)
      expect(Object.keys(downloadedResolved.download.knownValues)).to.have.length(49)
    })
  })

  describe('resolved scene', () => {
    it('marks position to sceneId correctly', () => {
      expect(Object.keys(resolved.sceneInfo.positionToSceneId)).to.have.length(49)
      expect(resolved.sceneInfo.positionToSceneId['0,0']).to.eq(scene.id)
      expect(resolved.sceneInfo.sceneIdToPositions[scene.id]).to.include('1,1')
      expect(resolved.sceneInfo.sceneIdToMappings[scene.id][0].file).to.eq('filename')
      expect(resolved.sceneInfo.sceneIdToSceneJson[scene.id].name).to.eq('Scene')
    })

    it('marks sight count for the scene correctly', () => {
      const afterUpdate = rootReducer(
        resolved,
        processSceneSightChange(generateSightMap(resolved.sightInfo.inSight, resolved.sceneInfo.positionToSceneId))
      )
      expect(afterUpdate.sceneSight[scene.id]).to.eq(29)
    })
  })

  describe('spawn', () => {
    it('marks position to sceneId correctly', () => {
      expect(shouldSelectSpawnTarget(resolved)).to.eq(true)
    })

    it('marks sight count for the scene correctly', () => {
      const afterUpdate = rootReducer(
        afterSpawn,
        processSceneSightChange(generateSightMap(resolved.sightInfo.inSight, resolved.sceneInfo.positionToSceneId))
      )
      expect(afterUpdate.sceneSight[scene.id]).to.eq(29)
    })
  })

  describe('process parcel sight', () => {
    it('updates sight and lost sight', () => {
      const afterUpdate = rootReducer(afterSpawn, processParcelSightChange())
      const afterTeleport = rootReducer(
        rootReducer(afterUpdate, processUserTeleport('10,10')),
        processParcelSightChange()
      )

      expect(afterTeleport.sightInfo.recentlyLostSight).to.have.lengthOf(29)
      expect(afterTeleport.sightInfo.recentlySighted).to.have.lengthOf(29)
    })
  })
})
