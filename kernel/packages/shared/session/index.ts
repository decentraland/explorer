import { future, IFuture } from 'fp-future'
import { disconnect, sendToMordor } from '../comms'
import { removeUserProfile } from '../comms/peers'
import { globalDCL } from '../globalDCL'
import { bringDownClientAndShowError } from '../loading/ReportFatalError'
import { NEW_LOGIN } from '../loading/types'

export class Session {
  private static _instance: IFuture<Session> = future()

  static get current() {
    return Session._instance
  }

  async logout() {
    globalDCL.rendererInterface.SetLoadingScreenVisible(true)
    sendToMordor()
    disconnect()
    removeUserProfile()
    window.location.reload()
  }

  disable() {
    bringDownClientAndShowError(NEW_LOGIN)
    sendToMordor()
    disconnect()
  }
}
