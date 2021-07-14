import { AnyAction } from 'redux'

import { COMMS_ESTABLISHED } from 'shared/loading/types'

import { CommsState, VoicePolicy } from './types'
import { SET_COMMS_ISLAND, SET_VOICE_CHAT_RECORDING, SET_VOICE_POLICY, TOGGLE_VOICE_CHAT_RECORDING } from './actions'
import { PREFERED_ISLAND } from 'config'

const INITIAL_COMMS = {
  initialized: false,
  voiceChatRecording: false,
  voicePolicy: VoicePolicy.ALLOW_ALL,
  preferedIsland: PREFERED_ISLAND ?? undefined
}

export function commsReducer(state?: CommsState, action?: AnyAction): CommsState {
  if (!state) {
    return INITIAL_COMMS
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    case COMMS_ESTABLISHED:
      return { ...state, initialized: true }
    case SET_VOICE_CHAT_RECORDING:
      return { ...state, voiceChatRecording: action.payload.recording }
    case TOGGLE_VOICE_CHAT_RECORDING:
      return { ...state, voiceChatRecording: !state.voiceChatRecording }
    case SET_VOICE_POLICY:
      return { ...state, voicePolicy: action.payload.voicePolicy }
    case SET_COMMS_ISLAND:
      return { ...state, island: action.payload.island }
    default:
      return state
  }
}
