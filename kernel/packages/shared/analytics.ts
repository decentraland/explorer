import { DEBUG_ANALYTICS } from 'config'

import { worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { Vector2 } from 'decentraland-ecs/src'
import { defaultLogger } from 'shared/logger'

import { avatarMessageObservable } from './comms/peers'
import { AvatarMessageType } from './comms/interface/types'
import { positionObservable } from './world/positionThings'
import { trackingEventObservable } from './observables'

export type SegmentEvent = {
  name: string
  data: string
}

export function trackEvent(eventName: string, eventData: Record<string, any>) {
  if (DEBUG_ANALYTICS) {
    defaultLogger.info(`Tracking event "${eventName}": `, eventData)
  }

  trackingEventObservable.notifyObservers({
    eventName,
    eventData
  })
}

const TRACEABLE_AVATAR_EVENTS = [
  AvatarMessageType.ADD_FRIEND,
  AvatarMessageType.SET_LOCAL_UUID,
  AvatarMessageType.USER_MUTED,
  AvatarMessageType.USER_UNMUTED,
  AvatarMessageType.USER_BLOCKED,
  AvatarMessageType.USER_UNBLOCKED
]

export function hookAnalyticsObservables() {
  avatarMessageObservable.add(({ type, ...data }) => {
    if (!TRACEABLE_AVATAR_EVENTS.includes(type)) {
      return
    }

    trackEvent(type, data)
  })

  let lastTime: number = performance.now()

  let previousPosition: string | null = null
  const gridPosition = Vector2.Zero()

  positionObservable.add(({ position }) => {
    // Update seconds variable and check if new parcel
    if (performance.now() - lastTime > 1000) {
      worldToGrid(position, gridPosition)
      const currentPosition = `${gridPosition.x | 0},${gridPosition.y | 0}`
      if (previousPosition !== currentPosition) {
        trackEvent('Move to Parcel', {
          newParcel: currentPosition,
          oldParcel: previousPosition,
          exactPosition: { x: position.x, y: position.y, z: position.z }
        })
        previousPosition = currentPosition
      }
      lastTime = performance.now()
    }
  })
}
