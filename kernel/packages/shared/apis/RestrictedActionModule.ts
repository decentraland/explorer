import { exposeMethod, registerAPI } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import defaultLogger from '../logger'
import { unityInterface } from '../../unity-interface/UnityInterface'
import { ParcelIdentity } from './ParcelIdentity'
import { Vector3 } from '../../decentraland-ecs/src/decentraland/math'
import { gridToWorld, isInParcel, parseParcelPosition } from '../../atomicHelpers/parcelScenePositions'

enum Permission {
  ALLOW_MOVE_INSIDE_SCENE = 'ALLOW_MOVE_INSIDE_SCENE'
}

export interface IRestrictedActionModule {
  requestMoveTo(newPosition: Vector3): Promise<void>
}

@registerAPI('RestrictedActionModule')
export class RestrictedActionModule extends ExposableAPI implements IRestrictedActionModule {
  parcelIdentity = this.options.getAPIInstance(ParcelIdentity)

  getSceneData() {
    return this.parcelIdentity.land.sceneJsonData
  }

  hasPermission(permission: Permission) {
    const json = this.getSceneData()
    const list = json.requiredPermissions || []
    return list.indexOf(permission) !== -1
  }

  calculatePosition(newPosition: Vector3) {
    const base = parseParcelPosition(this.getSceneData().scene.base)

    const basePosition = new Vector3()
    gridToWorld(base.x, base.y, basePosition)

    return basePosition.add(newPosition)
  }

  isPositionValid(position: Vector3) {
    return this.getSceneData().scene.parcels.some((parcel) => {
      const { x, y } = parseParcelPosition(parcel)
      return isInParcel(position, gridToWorld(x, y, new Vector3()))
    })
  }

  @exposeMethod
  async requestMoveTo(newPosition: Vector3, cameraTarget?: Vector3): Promise<void> {
    // validar que tenga permission
    if (!this.hasPermission(Permission.ALLOW_MOVE_INSIDE_SCENE)) {
      defaultLogger.error(`Permission "${Permission.ALLOW_MOVE_INSIDE_SCENE}" is required`)
      return
    }
    // calcular nueva position
    const position = this.calculatePosition(newPosition)
    // validate new position is inside of any scene
    if (!this.isPositionValid(position)) {
      defaultLogger.error('Error: Position is out of scene')
      return
    }
    unityInterface.Teleport({ position, cameraTarget })
  }
}
