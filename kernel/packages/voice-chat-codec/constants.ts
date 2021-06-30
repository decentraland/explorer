declare const globalThis: { ROOT_URL?: string }

export const OPUS_BITS_PER_SECOND = 24000
export const OPUS_FRAME_SIZE_MS = 40

export const VOICE_CHAT_SAMPLE_RATE = 24000

export const OPUS_SAMPLES_PER_FRAME = (VOICE_CHAT_SAMPLE_RATE * OPUS_FRAME_SIZE_MS) / 1000

export const OUTPUT_NODE_BUFFER_SIZE = 2048
export const OUTPUT_NODE_BUFFER_DURATION = (OUTPUT_NODE_BUFFER_SIZE * 1000) / VOICE_CHAT_SAMPLE_RATE

export const INPUT_NODE_BUFFER_SIZE = 2048

export const getVoiceChatCDNRootUrl = (): string => {
  if (typeof globalThis.ROOT_URL === 'undefined') {
    // NOTE(Brian): In branch urls we can't just use location.source - the value returned doesn't include
    //              the branch full path! With this, we ensure the /branch/<branch-name> is included in the root url.
    //              This is used for empty parcels and should be used for fetching any other local resource.
    return `voice-chat-codec`
  } else {
    return new URL(globalThis.ROOT_URL + 'voice-chat-codec', document.location.toString()).toString()
  }
}
