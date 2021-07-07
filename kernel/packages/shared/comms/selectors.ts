import { RootCommsState } from './types'

export const isVoiceChatRecording = (store: RootCommsState) => store.comms.voiceChatRecording

export const getVoicePolicy = (store: RootCommsState) => store.comms.voicePolicy

export const getCommsIsland = (store: RootCommsState) => store.comms.island