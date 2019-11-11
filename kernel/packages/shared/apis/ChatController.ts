// tslint:disable:prefer-function-over-method
import { Vector3Component } from 'atomicHelpers/landHelpers'
import { uuid } from 'atomicHelpers/math'
import { parseParcelPosition, worldToGrid } from 'atomicHelpers/parcelScenePositions'
import { parcelLimits } from 'config'
import { APIOptions, exposeMethod, registerAPI } from 'decentraland-rpc/lib/host'
import { EngineAPI } from 'shared/apis/EngineAPI'
import { ExposableAPI } from 'shared/apis/ExposableAPI'
import { sendPublicChatMessage } from 'shared/comms'
import { ChatEvent, chatObservable } from 'shared/comms/chat'
import {
  addToMutedUsers,
  avatarMessageObservable,
  findPeerByName,
  getCurrentUser,
  peerMap,
  removeFromMutedUsers
} from 'shared/comms/peers'
import { AvatarMessage, AvatarMessageType } from 'shared/comms/types'
import { IChatCommand, MessageEntry } from 'shared/types'
import { teleportObservable } from 'shared/world/positionThings'

const userPose: { [key: string]: Vector3Component } = {}
avatarMessageObservable.add((pose: AvatarMessage) => {
  if (pose.type === AvatarMessageType.USER_POSE) {
    userPose[pose.uuid] = { x: pose.pose[0], y: pose.pose[1], z: pose.pose[2] }
  }
  if (pose.type === AvatarMessageType.USER_REMOVED) {
    delete userPose[pose.uuid]
  }
})

export interface IChatController {
  /**
   * Send the chat message
   * @param messageEntry
   */
  send(message: string): Promise<MessageEntry>
  /**
   * Return list of chat commands
   */
  getChatCommands(): Promise<{ [key: string]: IChatCommand }>
}

@registerAPI('ChatController')
export class ChatController extends ExposableAPI implements IChatController {
  private chatCommands: { [key: string]: IChatCommand } = {}

  constructor(options: APIOptions) {
    super(options)
    this.initChatCommands()

    const engineAPI = options.getAPIInstance(EngineAPI)

    chatObservable.add((event: any) => {
      if (event.type === ChatEvent.MESSAGE_RECEIVED || event.type === ChatEvent.MESSAGE_SENT) {
        const { type, ...data } = event
        engineAPI.sendSubscriptionEvent(event.type, data)
      }
    })
  }

  @exposeMethod
  async send(message: string): Promise<MessageEntry> {
    let entry

    // Check if message is a command
    if (message[0] === '/') {
      entry = this.handleChatCommand(message)

      // If no such command was found, provide some feedback
      if (!entry) {
        entry = {
          id: uuid(),
          isCommand: true,
          sender: 'Decentraland',
          message: `That command doesn’t exist. Type /help for a full list of commands.`
        }
      }
    } else {
      // If the message was not a command ("/cmdname"), then send message through wire
      const currentUser = getCurrentUser()
      if (!currentUser) throw new Error('cannotGetCurrentUser')
      if (!currentUser.profile) throw new Error('profileNotInitialized')
      const newEntry = (entry = {
        id: uuid(),
        isCommand: false,
        sender: currentUser.profile.name || currentUser.userId || 'unknown',
        message
      })

      sendPublicChatMessage(newEntry.id, newEntry.message)
    }

    return entry
  }

  @exposeMethod
  async getChatCommands() {
    return this.chatCommands
  }

  // @internal
  handleChatCommand(message: string) {
    const words = message
      .substring(0, message.length - 1) // Remove \n character
      .split(' ')

    const command = words[0].substring(1).trim()
    // Remove command from sentence
    words.shift()
    const restOfMessage = words.join(' ')

    const cmd = this.chatCommands[command]

    if (cmd) {
      return cmd.run(restOfMessage)
    }

    return null
  }

  // @internal
  addChatCommand(name: string, description: string, fn: (message: string) => MessageEntry): void {
    if (this.chatCommands[name]) {
      // Chat command already registered
      return
    }

    this.chatCommands[name] = {
      name,
      description,
      run: message => fn(message)
    }
  }

  // @internal
  initChatCommands() {
    this.addChatCommand('goto', 'Teleport to another parcel', message => {
      const coordinates = parseParcelPosition(message)
      const isValid = isFinite(coordinates.x) && isFinite(coordinates.y)

      let response = ''

      if (!isValid) {
        response = 'Could not recognize the coordinates provided. Example usage: /goto 42,42'
      } else {
        const { x, y } = coordinates

        if (
          parcelLimits.minLandCoordinateX <= x &&
          x <= parcelLimits.maxLandCoordinateX &&
          parcelLimits.minLandCoordinateY <= y &&
          y <= parcelLimits.maxLandCoordinateY
        ) {
          teleportObservable.notifyObservers({ x, y })
          response = `Teleporting to ${x}, ${y}...`
        } else {
          response = `Coordinates are outside of the boundaries. Limits are from ${parcelLimits.minLandCoordinateX} to ${parcelLimits.maxLandCoordinateX} for X and ${parcelLimits.minLandCoordinateY} to ${parcelLimits.maxLandCoordinateY} for Y`
        }
      }

      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: response
      }
    })

    this.addChatCommand('players', 'Shows a list of players around you', message => {
      const users = [...peerMap.entries()]

      const strings = users
        .filter(([_, value]) => !!(value && value.user && value.user.profile && value.user.profile.name))
        .filter(([uuid]) => userPose[uuid])
        .map(function([uuid, value]) {
          const pos = { x: 0, y: 0 }
          worldToGrid(userPose[uuid], pos)
          return `  ${value.user!.profile!.name}: ${pos.x}, ${pos.y}`
        })
        .join('\n')
      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: strings ? `Players around you:\n${strings}` : 'No other players are near to your location'
      }
    })

    this.addChatCommand('getname', 'Gets your username', message => {
      const currentUser = getCurrentUser()
      if (!currentUser) throw new Error('cannotGetCurrentUser')
      if (!currentUser.profile) throw new Error('profileNotInitialized')
      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message: `Your Display Name is ${currentUser.profile.name}.`
      }
    })

    this.addChatCommand('mute', 'Mute [username]', message => {
      const username = message
      const currentUser = getCurrentUser()
      if (!currentUser) throw new Error('cannotGetCurrentUser')

      const user = findPeerByName(username)
      if (user && user.userId) {
        // Cannot mute yourself
        if (username === currentUser.userId) {
          return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `You cannot mute yourself.` }
        }

        addToMutedUsers(user.userId)

        return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `You muted user ${username}.` }
      } else {
        return {
          id: uuid(),
          isCommand: true,
          sender: 'Decentraland',
          message: `User not found ${JSON.stringify(username)}.`
        }
      }
    })

    this.addChatCommand('unmute', 'Unmute [username]', message => {
      const username = message
      const currentUser = getCurrentUser()
      if (!currentUser) throw new Error('cannotGetCurrentUser')

      const user = findPeerByName(username)

      if (user && user.userId) {
        // Cannot unmute or mute yourself
        if (username === currentUser.userId) {
          return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `You cannot mute or unmute yourself.` }
        }

        removeFromMutedUsers(user.userId)

        return { id: uuid(), isCommand: true, sender: 'Decentraland', message: `You unmuted user ${username}.` }
      } else {
        return {
          id: uuid(),
          isCommand: true,
          sender: 'Decentraland',
          message: `User not found ${JSON.stringify(username)}.`
        }
      }
    })

    this.addChatCommand('help', 'Show a list of commands', message => {
      return {
        id: uuid(),
        isCommand: true,
        sender: 'Decentraland',
        message:
          `Click on the screen to lock the cursor, later you can unlock it with the [ESC] key.` +
          `\n\nYou can move with the [WASD] keys and jump with the [SPACE] key.` +
          `\n\nYou can toggle the chat with the [ENTER] key.` +
          `\n\nAvailable commands:\n${Object.keys(this.chatCommands)
            .filter(name => name !== 'help')
            .map(name => `\t/${name}: ${this.chatCommands[name].description}`)
            .concat('\t/help: Show this list of commands')
            .join('\n')}`
      }
    })
  }
}
