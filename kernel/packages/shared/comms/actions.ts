import { action } from 'typesafe-actions'

export const VOICE_PLAYING_UPDATE = 'Voice Playing Update'
export const voicePlayingUpdate = (userId: string, playing: boolean) =>
  action(VOICE_PLAYING_UPDATE, { userId, playing })
export type VoicePlayingUpdate = ReturnType<typeof voicePlayingUpdate>

export const SET_VOICE_CHAT_RECORDING = 'Set Voice Chat Recording'
export const setVoiceChatRecording = (recording: boolean) => action(SET_VOICE_CHAT_RECORDING, { recording })
export type SetVoiceChatRecording = ReturnType<typeof setVoiceChatRecording>
