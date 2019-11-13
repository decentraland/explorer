import { Vector2Component } from 'atomicHelpers/landHelpers'
import { SceneLifeCycleController } from './scene'
import { EventEmitter } from 'events'
import { ParcelLifeCycleController } from './parcel'
import { SceneDataDownloadManager } from './download'
import { worldToGrid, gridToWorld } from '../../../atomicHelpers/parcelScenePositions'
import { pickWorldSpawnpoint } from 'shared/world/positionThings'
import { InstancedSpawnPoint } from 'shared/types'

export class PositionLifecycleController extends EventEmitter {
  private positionSettled: boolean = false
  private currentlySightedScenes: string[] = []
  private currentSpawnpoint?: InstancedSpawnPoint

  constructor(
    private downloadManager: SceneDataDownloadManager,
    private parcelController: ParcelLifeCycleController,
    private sceneController: SceneLifeCycleController
  ) {
    super()
    sceneController.on('Scene status', () => this.checkPositionSettlement())
  }

  async reportCurrentPosition(position: Vector2Component, teleported: boolean) {
    let resolvedPosition = position
    if (teleported) {
      const land = await this.downloadManager.getParcelData(`${position.x},${position.y}`)
      if (land) {
        const spawnPoint = pickWorldSpawnpoint(land)
        resolvedPosition = worldToGrid(spawnPoint.position)
        this.queueTrackingEvent('Scene Spawn', { parcel: land.scene.scene.base, spawnpoint: spawnPoint.position })

        this.currentSpawnpoint = spawnPoint
      } else {
        this.currentSpawnpoint = { position: gridToWorld(position.x, position.y) }
      }
    }

    const parcels = this.parcelController.reportCurrentPosition(resolvedPosition)

    if (parcels) {
      const newlySightedScenes = await this.sceneController.reportSightedParcels(parcels.sighted, parcels.lostSight)

      if (!this.eqSet(this.currentlySightedScenes, newlySightedScenes.sighted)) {
        this.currentlySightedScenes = newlySightedScenes.sighted
      }
    }

    if (teleported) {
      this.positionSettled = false
      this.emit('Unsettled Position')
    }

    this.checkPositionSettlement()
  }

  private eqSet(as: Array<any>, bs: Array<any>) {
    if (as.length !== bs.length) return false
    for (const a of as) if (!bs.includes(a)) return false
    return true
  }

  private checkPositionSettlement() {
    if (!this.positionSettled) {
      const settling = this.currentlySightedScenes.every($ => this.sceneController.isRenderable($))

      if (settling) {
        this.positionSettled = settling
        this.emit('Settled Position', this.currentSpawnpoint)
      }
    }
  }

  private queueTrackingEvent(name: string, data: any) {
    this.emit('Tracking Event', { name, data })
  }
}
