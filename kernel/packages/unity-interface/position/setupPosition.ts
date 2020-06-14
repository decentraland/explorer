import { playerConfigurations } from 'config'
import { Quaternion, Vector3 } from 'decentraland-ecs/src/decentraland/math'
import { globalDCL } from 'shared/globalDCL'
import { teleportTriggered } from 'shared/loading/types'
import { teleportObservable } from 'shared/world/positionThings'
import { worldRunningObservable } from 'shared/world/worldState'

export const cachedPositionEvent = {
  position: Vector3.Zero(),
  quaternion: Quaternion.Identity,
  rotation: Vector3.Zero(),
  playerHeight: playerConfigurations.height,
  mousePosition: Vector3.Zero()
}

export function setupPosition() {
  teleportObservable.add((position: { x: number; y: number; text?: string }) => {
    // before setting the new position, show loading screen to avoid showing an empty world
    globalDCL.rendererInterface.SetLoadingScreenVisible(true)
    globalDCL.globalStore.dispatch(teleportTriggered(position.text || `Teleporting to ${position.x}, ${position.y}`))
  })

  worldRunningObservable.add(isRunning => {
    if (isRunning) {
      globalDCL.rendererInterface.SetLoadingScreenVisible(false)
    }
  })
}
