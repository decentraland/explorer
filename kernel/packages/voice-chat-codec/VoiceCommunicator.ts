import { VoiceChatCodecWorkerMain } from './VoiceChatCodecWorkerMain'
import { RingBuffer } from 'atomicHelpers/RingBuffer'
import { defer } from 'atomicHelpers/defer'

export type AudioCommunicatorChannel = {
  send(data: Uint8Array): any
}

export type StreamPlayingListener = (streamId: string, playing: boolean) => any

type VoiceOutput = {
  buffer: RingBuffer<Float32Array>
  scriptProcessor: ScriptProcessorNode
  playing: boolean
}

export class VoiceCommunicator {
  private context: AudioContext
  private inputProcessor: ScriptProcessorNode
  private input?: MediaStreamAudioSourceNode
  private voiceChatWorkerMain: VoiceChatCodecWorkerMain
  private outputs: Record<string, VoiceOutput> = {}

  private streamPlayingListeners: StreamPlayingListener[] = []

  private readonly sampleRate = 48000
  private readonly channelBufferSize = 0.8

  constructor(private selfId: string, private channel: AudioCommunicatorChannel) {
    this.context = new AudioContext({ sampleRate: this.sampleRate })
    this.inputProcessor = this.context.createScriptProcessor(2048, 1, 1)
    this.voiceChatWorkerMain = new VoiceChatCodecWorkerMain()
    this.createEncodeStream()
  }

  public setSelfId(selfId: string) {
    this.voiceChatWorkerMain.destroyEncodeStream(this.selfId)
    this.selfId = selfId
    this.createEncodeStream()
  }

  public addStreamPlayingListener(listener: StreamPlayingListener) {
    this.streamPlayingListeners.push(listener)
  }

  public hasInput() {
    return !!this.input
  }

  private createEncodeStream() {
    const encodeStream = this.voiceChatWorkerMain.getOrCreateEncodeStream(this.selfId, this.sampleRate)
    encodeStream.addAudioEncodedListener((data) => this.channel.send(data))

    this.inputProcessor.onaudioprocess = async function (e) {
      const buffer = e.inputBuffer
      encodeStream.encode(buffer.getChannelData(0))
    }
  }

  async playEncodedAudio(src: string, encoded: Uint8Array) {
    if (!this.outputs[src]) {
      this.outputs[src] = {
        buffer: new RingBuffer(Math.floor(this.channelBufferSize * this.sampleRate), Float32Array),
        scriptProcessor: this.createScriptOutputFor(src),
        playing: false
      }
    }

    let stream = this.voiceChatWorkerMain.decodeStreams[src]

    if (!stream) {
      stream = this.voiceChatWorkerMain.getOrCreateDecodeStream(src, this.sampleRate)

      stream.addAudioDecodedListener((samples) => this.outputs[src].buffer.write(samples))
    }

    stream.decode(encoded)
  }

  createScriptOutputFor(src: string) {
    const bufferSize = 8192
    const processor = this.context.createScriptProcessor(bufferSize, 0, 1)
    processor.onaudioprocess = (ev) => {
      const data = ev.outputBuffer.getChannelData(0)

      data.fill(0)
      if (this.outputs[src]) {
        const wasPlaying = this.outputs[src].playing
        if (this.outputs[src].buffer.readAvailableCount() > bufferSize / 2) {
          data.set(this.outputs[src].buffer.read(data.length))
          if (!wasPlaying) {
            this.changePlayingStatus(src, true)
          }
        } else {
          if (wasPlaying) {
            this.changePlayingStatus(src, false)
          }
        }
      }
    }

    processor.connect(this.context.destination)

    return processor
  }

  changePlayingStatus(streamId: string, playing: boolean) {
    this.outputs[streamId].playing = playing
    // Listeners could be long running, so we defer the execution of them
    defer(() => {
      this.streamPlayingListeners.forEach((listener) => listener(streamId, playing))
    })
  }

  setInputStream(stream: MediaStream) {
    this.input = this.context.createMediaStreamSource(stream)
    this.input.connect(this.inputProcessor)
  }

  start() {
    if (this.input) {
      this.inputProcessor.connect(this.context.destination)
    }
  }

  pause() {
    if (this.input) {
      this.inputProcessor.disconnect(this.context.destination)
    }
  }
}
