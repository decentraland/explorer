import { RootCommsState } from './types'

export const isVoiceChatRecording = (store: RootCommsState) => store.comms.voiceChatRecording

export const getVoicePolicy = (store: RootCommsState) => store.comms.voicePolicy

export const getCommsIsland = (store: RootCommsState) => store.comms.island

export const getPreferedIsland = (store: RootCommsState) => store.comms.preferedIsland
