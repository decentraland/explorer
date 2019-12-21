import { WorldInstanceConnection } from '../../comms-interface/index'
import { Stats } from '../debug'
import { Package, BusMessage, ChatMessage, ProfileVersion, UserInformation } from '../../comms-interface/types'
import { Position } from '../../comms-interface/utils'
import { Peer } from 'decentraland-katalyst-peer'

const NOOP = () => {
  // do nothing
}

export class LighthouseWorldInstanceConnection implements WorldInstanceConnection {
  stats: Stats | null = null

  sceneMessageHandler: (alias: string, data: Package<BusMessage>) => void = NOOP
  chatHandler: (alias: string, data: Package<ChatMessage>) => void = NOOP
  profileHandler: (alias: string, identity: string, data: Package<ProfileVersion>) => void = NOOP
  positionHandler: (alias: string, data: Package<Position>) => void = NOOP

  isAuthenticated: boolean = true // TODO - remove this

  ping: number = -1

  // @ts-ignore
  constructor(private peer: Peer) {
    // nothing to do here
  }

  printDebugInformation() {
    // TODO - implement this - moliva - 20/12/2019
  }

  close() {
    // TODO - implement this - moliva - 20/12/2019
  }

  async sendInitialMessage(userInfo: Partial<UserInformation>) {
    // TODO - implement this - moliva - 20/12/2019
  }

  async sendProfileMessage(currentPosition: Position, userInfo: UserInformation) {
    // TODO - implement this - moliva - 20/12/2019
  }

  async sendPositionMessage(p: Position) {
    // TODO - implement this - moliva - 20/12/2019
  }

  async sendParcelUpdateMessage(currentPosition: Position, p: Position) {
    // TODO - implement this - moliva - 20/12/2019
  }

  async sendParcelSceneCommsMessage(cid: string, message: string) {
    // TODO - implement this - moliva - 20/12/2019
  }

  async sendChatMessage(currentPosition: Position, messageId: string, text: string) {
    // TODO - implement this - moliva - 20/12/2019
  }

  async updateSubscriptions(rooms: string[]) {
    const currentRooms = this.peer.currentRooms
    const joining = rooms.map(room => {
      if (!currentRooms.some(current => current.id === room)) {
        return this.peer.joinRoom(room)
      } else {
        return Promise.resolve()
      }
    })
    const leaving = currentRooms.map(current => {
      if (!rooms.some(room => current.id === room)) {
        return this.peer.leaveRoom(current.id)
      } else {
        return Promise.resolve()
      }
    })
    return Promise.all([...joining, ...leaving]).then(NOOP)
  }
}
