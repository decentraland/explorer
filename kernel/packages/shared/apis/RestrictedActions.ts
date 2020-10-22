import { exposeMethod, registerAPI } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import defaultLogger from '../logger'
import { unityInterface } from 'unity-interface/UnityInterface'
import { ParcelIdentity } from './ParcelIdentity'
import { Quaternion, Vector3 } from 'decentraland-ecs/src'
import { gridToWorld, isInParcel, parseParcelPosition } from '../../atomicHelpers/parcelScenePositions'
import { lastPlayerPosition } from '../world/positionThings'
import { browserInterface } from "../../unity-interface/BrowserInterface"

@registerAPI('RestrictedActions')
export class RestrictedActions extends ExposableAPI {
  parcelIdentity = this.options.getAPIInstance(ParcelIdentity)

  /**
   * move player to a position inside the scene
   *
   * @param position PositionType
   * @param cameraTarget PositionType
   */
  @exposeMethod
  async movePlayerTo(newPosition: Vector3, cameraTarget?: Vector3): Promise<void> {
    // checks permissions
    if (!this.hasPermission(Permission.ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE)) {
      defaultLogger.error(`Permission "${Permission.ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE}" is required`)
      return
    }

    const position = this.calculatePosition(newPosition)

    // validate new position is inside of some scene
    if (!this.isPositionValid(position)) {
      defaultLogger.error('Error: Position is out of scene', position)
      return
    }
    if (!this.isPositionValid(lastPlayerPosition)) {
      defaultLogger.error('Error: Player is not inside of scene', lastPlayerPosition)
      return
    }

    unityInterface.Teleport({ position, cameraTarget }, false)

    // Get ahead of the position report that will be done automatically later and report
    // position right now, also marked as an immediate update (last bool in Position structure)
    browserInterface.ReportPosition({
      position: newPosition,
      rotation: Quaternion.Identity,
      immediate: true
    })
  }

  /**
   * trigger an emote on the current player
   *
   * @param emote the emote to perform
   */
  @exposeMethod
  async triggerEmote(emote: Emote): Promise<void> {
    // checks permissions
    if (!this.hasPermission(Permission.ALLOW_TO_TRIGGER_AVATAR_EMOTE)) {
      defaultLogger.error(`Permission "${Permission.ALLOW_TO_TRIGGER_AVATAR_EMOTE}" is required`)
      return
    }

    unityInterface.TriggerSelfUserExpression(this.translatePredefinedEmoteToId(emote.predefined))
  }

  private translatePredefinedEmoteToId(emoteName: string): string {
    switch (emoteName) {
      case 'WAVE': return 'wave';
      case 'FIST_PUMP': return 'fistpump';
      case 'ROBOT': return 'robot';
      case 'RAISE_HAND': return 'raiseHand';
      case 'CLAP': return 'clap';
      case 'MONEY': return 'money';
      case 'KISS': return 'kiss';
    }
    throw new Error(`Unrecognized emote name ${emoteName}`)
  }

  private getSceneData() {
    return this.parcelIdentity.land.sceneJsonData
  }

  private hasPermission(permission: Permission) {
    const json = this.getSceneData()
    const list = json.requiredPermissions || []
    return list.indexOf(permission) !== -1
  }

  private calculatePosition(newPosition: Vector3) {
    const base = parseParcelPosition(this.getSceneData().scene.base)

    const basePosition = new Vector3()
    gridToWorld(base.x, base.y, basePosition)

    return basePosition.add(newPosition)
  }

  private isPositionValid(position: Vector3) {
    return this.getSceneData().scene.parcels.some((parcel) => {
      const { x, y } = parseParcelPosition(parcel)
      return isInParcel(position, gridToWorld(x, y))
    })
  }
}

type Emote = {
  predefined: PredefinedEmote
}

type PredefinedEmote = string

export enum Permission {
  ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE = 'ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE',
  ALLOW_TO_TRIGGER_AVATAR_EMOTE = 'ALLOW_TO_TRIGGER_AVATAR_EMOTE'
}
