import { DecentralandInterface } from 'decentraland-ecs/src/decentraland/Types'
import { Color4 } from 'decentraland-ecs/src'
import { OnTextSubmit, OnBlur, OnFocus } from 'decentraland-ecs/src/decentraland/UIEvents'

import {
  UIInputText,
  UIText,
  UIContainerStack,
  UIContainerRect,
  UIShape,
  UIScrollRect
} from 'decentraland-ecs/src/decentraland/UIShapes'

import { MessageEntry } from 'shared/types'

import { execute } from './rpc'
import { screenSpaceUI } from './ui'

declare var dcl: DecentralandInterface

const INITIAL_INPUT_TEXT_COLOR = Color4.White()
const PRIMARY_TEXT_COLOR = Color4.White()
const COMMAND_COLOR = Color4.FromHexString('#80ffe5ff')
const MAX_LOGGED_MESSAGES = 10

// UI creators -------------------
dcl.subscribe('MESSAGE_RECEIVED')
dcl.subscribe('MESSAGE_SENT')
dcl.onEvent(event => {
  const eventType: string = event.type
  const eventData: any = event.data
  if (eventType === 'MESSAGE_RECEIVED' || eventType === 'MESSAGE_SENT') {
    addMessage(eventData.messageEntry as MessageEntry)
  }
})

function createLogMessage(parent: UIShape, props: { sender: string; message: string; isCommand?: boolean }) {
  const { sender, message, isCommand } = props
  const color = isCommand ? COMMAND_COLOR : PRIMARY_TEXT_COLOR

  const messageText = new UIText(parent)
  messageText.color = color
  messageText.value = `<b>${sender}:</b> ${message}`
  messageText.fontSize = 14
  messageText.vAlign = 'top'
  messageText.hAlign = 'left'
  messageText.vTextAlign = 'top'
  messageText.hTextAlign = 'left'
  messageText.width = '350px'
  messageText.adaptWidth = false
  messageText.adaptHeight = true
  messageText.textWrapping = true
  messageText.outlineColor = Color4.Black()

  internalState.loggedMessages.push(messageText)

  messagesLogScrollContainer.valueY = 0

  return { component: messageText }
}

function updateLogMessage(index: number, message: MessageEntry) {
  if(!internalState.loggedMessages[index]) return

  internalState.loggedMessages[index].value = `<b>${message.sender}:</b> ${message.message}`
}

// -------------------------------
const internalState = {
  commandsList: [] as Array<any>,
  messages: [] as Array<any>,
  loggedMessages: [] as Array<UIText>,
  isFocused: false,
  isSliderVisible: false
}

let isMaximized: boolean = false

const chatContainer = new UIContainerRect(screenSpaceUI)
chatContainer.name = 'chat-container'
chatContainer.color = Color4.Clear()
chatContainer.vAlign = 'bottom'
chatContainer.hAlign = 'left'
chatContainer.width = '380px'
chatContainer.height = '250px'
chatContainer.positionX = 10
chatContainer.positionY = 10
chatContainer.thickness = 0

const chatInnerTopContainer = new UIContainerRect(chatContainer)
chatInnerTopContainer.color = Color4.Clear()
chatInnerTopContainer.name = 'inner-top-container'
chatInnerTopContainer.vAlign = 'top'
chatInnerTopContainer.hAlign = 'left'
chatInnerTopContainer.width = '100%'
chatInnerTopContainer.height = '82.5%'

const messagesLogScrollContainer = new UIScrollRect(chatInnerTopContainer)
messagesLogScrollContainer.name = 'messages-log-scroll-container'
messagesLogScrollContainer.vAlign = 'top'
messagesLogScrollContainer.hAlign = 'left'
messagesLogScrollContainer.width = '100%'
messagesLogScrollContainer.height = '90%'
messagesLogScrollContainer.positionY = '-8px'
messagesLogScrollContainer.positionX = -5
messagesLogScrollContainer.valueY = 1
messagesLogScrollContainer.isVertical = false
messagesLogScrollContainer.isHorizontal = false
messagesLogScrollContainer.visible = true

const messagesLogStackContainer = new UIContainerStack(messagesLogScrollContainer)
messagesLogStackContainer.name = 'messages-log-stack-container'
messagesLogStackContainer.vAlign = 'bottom'
messagesLogStackContainer.hAlign = 'center'
messagesLogStackContainer.width = '100%'
messagesLogStackContainer.height = '100%'
messagesLogStackContainer.spacing = 5//inter message
messagesLogStackContainer.positionX = 4//position about the box

const textInputContainer = new UIContainerRect(chatContainer)
textInputContainer.color = Color4.Clear()
textInputContainer.name = 'input-text-container'
textInputContainer.vAlign = 'bottom'
textInputContainer.hAlign = 'left'
textInputContainer.width = '100%'
textInputContainer.height = '16%'

const textInput = new UIInputText(textInputContainer)
textInput.name = 'input-text'
textInput.autoStretchWidth = false
textInput.color = INITIAL_INPUT_TEXT_COLOR
textInput.background = Color4.Clear()
textInput.focusedBackground = Color4.Clear()
textInput.placeholder = 'Press enter and start talking...'
textInput.fontSize = 14
textInput.width = '90%'
textInput.height = '16%'
textInput.thickness = 0
textInput.vAlign = 'center'
textInput.hAlign = 'center'
textInput.positionX = '-5px'
textInput.vTextAlign = 'center'
textInput.hTextAlign = 'left'
textInput.value = ''
textInput.textWrapping = true
textInput.isPointerBlocker = true
textInput.onFocus = new OnFocus(onInputFocus)
textInput.onBlur = new OnBlur(onInputBlur)
textInput.onTextSubmit = new OnTextSubmit(onInputSubmit)

setMaximized(isMaximized)

const instructionsMessage = {
  id: '',
  isCommand: true,
  sender: 'Decentraland',
  message: 'Type /help for info about controls'
}
addMessage(instructionsMessage as MessageEntry)

export async function initializeChat() {
  const chatCmds = await execute('ChatController', 'getChatCommands', [null])
  const commandsList = []

  for (let i in chatCmds) {
    commandsList.push(chatCmds[i])
  }
}

function setMaximized(newMaximizedValue: boolean) {
  if (isMaximized == newMaximizedValue) return

  if (newMaximizedValue && !isMaximized) {
    textInput.value = ''

    chatInnerTopContainer.color = new Color4(0, 0, 0, 0.2)
    textInputContainer.color = new Color4(0, 0, 0, 0.2)
  } else if (isMaximized) {
    chatInnerTopContainer.color = Color4.Clear()
    textInputContainer.color = Color4.Clear()
  }

  isMaximized = newMaximizedValue

  messagesLogScrollContainer.isVertical = isMaximized
}

function onInputFocus() {
  setMaximized(true)
}

function onInputBlur() {
  setMaximized(false)
}

async function onInputSubmit(e: { text: string }) {
  await sendMsg(e.text)
}

async function sendMsg(messageToSend: string) {
  if (messageToSend) {
    const message = await execute('ChatController', 'send', [messageToSend])

    if (message) {
      addMessage(message)
    }
  }
}

function addMessage(messageEntry: MessageEntry): void {
  internalState.messages.push(messageEntry)

  if(internalState.messages.length <= MAX_LOGGED_MESSAGES) {
    createLogMessage(messagesLogStackContainer, messageEntry)
  } else {
    // remove oldest element
    internalState.messages.shift()

    // update logged messages ui text elements
    for (let index = 0; index < MAX_LOGGED_MESSAGES; index++) {
      updateLogMessage(index, internalState.messages[index])
    }
  }
}
