import { teleportObservable } from 'shared/world/positionThings'
import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { POIs } from 'shared/comms/POIs'
import { parcelLimits, tutorialEnabled } from 'config'
import { getUserProfile } from 'shared/comms/peers'
import { Profile } from 'shared/passports/types'
import { tutorialStepId } from 'decentraland-loader/lifecycle/tutorial/tutorial'

const CAMPAIGN_PARCEL_SEQUENCE = [
  { x: 113, y: -7 },
  { x: 87, y: 18 },
  { x: 52, y: 2 },
  { x: 16, y: 83 },
  { x: -12, y: -39 },
  { x: 60, y: 115 }
]

export class TeleportController {
  public static ensureTeleportAnimation() {
    document
      .getElementById('gameContainer')!
      .setAttribute(
        'style',
        'background: #151419 url(images/teleport.gif) no-repeat center !important; background-size: 194px 257px !important;'
      )
    document.body.setAttribute(
      'style',
      'background: #151419 url(images/teleport.gif) no-repeat center !important; background-size: 194px 257px !important;'
    )
  }

  public static stopTeleportAnimation() {
    document.getElementById('gameContainer')!.setAttribute('style', 'background: #151419')
    document.body.setAttribute('style', 'background: #151419')

    const profile = getUserProfile().profile as Profile

    if (!tutorialEnabled() || profile.tutorialStep !== tutorialStepId.INITIAL_SCENE) {
      ;(window as any)['unityInterface'].ShowWelcomeNotification()
    }
  }

  public static goToMagic(): { message: string; success: boolean } {
    const target = POIs[Math.floor(Math.random() * POIs.length)]
    const { x, y } = target
    const tpMessage: string = `Teleporting to "${target.name}" (${x}, ${y})...`
    return TeleportController.goTo(parseInt('' + x, 10), parseInt('' + y, 10), tpMessage)
  }

  public static goToRandom(): { message: string; success: boolean } {
    const x = Math.floor(Math.random() * 301) - 150
    const y = Math.floor(Math.random() * 301) - 150
    const tpMessage = `Teleporting to random location (${x}, ${y})...`
    return TeleportController.goTo(x, y, tpMessage)
  }

  public static goToNext(): { message: string; success: boolean } {
    const current = getFromLocalStorage('launch-campaign-status') || 0
    saveToLocalStorage('launch-campaign-status', current + 1)
    const { x, y } = CAMPAIGN_PARCEL_SEQUENCE[current % CAMPAIGN_PARCEL_SEQUENCE.length]
    return TeleportController.goTo(x, y, `Teleporting you to the next scene... and more treasures!`)
  }

  public static goTo(x: number, y: number, teleportMessage?: string): { message: string; success: boolean } {
    const tpMessage: string = teleportMessage ? teleportMessage : `Teleporting to ${x}, ${y}...`

    const insideCoords =
      x > parcelLimits.minLandCoordinateX &&
      x <= parcelLimits.maxLandCoordinateX &&
      y > parcelLimits.minLandCoordinateY &&
      y <= parcelLimits.maxLandCoordinateY

    if (insideCoords) {
      teleportObservable.notifyObservers({
        x: x,
        y: y,
        text: tpMessage
      } as any)

      TeleportController.ensureTeleportAnimation()

      return { message: tpMessage, success: true }
    } else {
      const errorMessage = `Coordinates are outside of the boundaries. Limits are from ${parcelLimits.minLandCoordinateX} to ${parcelLimits.maxLandCoordinateX} for X and ${parcelLimits.minLandCoordinateY} to ${parcelLimits.maxLandCoordinateY} for Y`
      return { message: errorMessage, success: false }
    }
  }
}
