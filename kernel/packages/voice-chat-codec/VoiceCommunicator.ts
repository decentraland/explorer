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
  panNode: PannerNode
  spatialParams: VoiceSpatialParams
  playing: boolean
}

export type VoiceCommunicatorOptions = {
  sampleRate?: number
  channelBufferSize?: number
  maxDistance?: number
  refDistance?: number
  initialListenerParams?: VoiceSpatialParams
}

export type VoiceSpatialParams = {
  position: [number, number, number]
  orientation: [number, number, number]
}

export class VoiceCommunicator {
  private context: AudioContext
  private inputProcessor: ScriptProcessorNode
  private input?: MediaStreamAudioSourceNode
  private voiceChatWorkerMain: VoiceChatCodecWorkerMain
  private outputs: Record<string, VoiceOutput> = {}

  private streamPlayingListeners: StreamPlayingListener[] = []

  private readonly sampleRate: number
  private readonly channelBufferSize: number

  constructor(
    private selfId: string,
    private channel: AudioCommunicatorChannel,
    private options: VoiceCommunicatorOptions
  ) {
    this.sampleRate = this.options.sampleRate ?? 20000
    this.channelBufferSize = this.options.channelBufferSize ?? 2.0

    this.context = new AudioContext({ sampleRate: this.sampleRate })

    if (this.options.initialListenerParams) {
      this.setListenerSpatialParams(this.options.initialListenerParams)
    }

    this.inputProcessor = this.context.createScriptProcessor(4096, 1, 1)
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

  async playEncodedAudio(src: string, relativePosition: VoiceSpatialParams, encoded: Uint8Array) {
    if (!this.outputs[src]) {
      const nodes = this.createOutputNodes(src)
      this.outputs[src] = {
        buffer: new RingBuffer(Math.floor(this.channelBufferSize * this.sampleRate), Float32Array),
        playing: false,
        spatialParams: relativePosition,
        ...nodes
      }
    } else {
      this.setVoiceRelativePosition(src, relativePosition)
    }

    let stream = this.voiceChatWorkerMain.decodeStreams[src]

    if (!stream) {
      stream = this.voiceChatWorkerMain.getOrCreateDecodeStream(src, this.sampleRate)

      stream.addAudioDecodedListener((samples) => this.outputs[src].buffer.write(samples))
    }

    stream.decode(encoded)
  }

  setListenerSpatialParams(spatialParams: VoiceSpatialParams) {
    const listener = this.context.listener
    listener.setPosition(spatialParams.position[0], spatialParams.position[1], spatialParams.position[2])
    listener.setOrientation(
      spatialParams.orientation[0],
      spatialParams.orientation[1],
      spatialParams.orientation[2],
      0,
      1,
      0
    )
  }

  updatePannerNodeParameters(src: string) {
    const panNode = this.outputs[src].panNode
    const spatialParams = this.outputs[src].spatialParams

    panNode.positionX.value = spatialParams.position[0]
    panNode.positionY.value = spatialParams.position[1]
    panNode.positionZ.value = spatialParams.position[2]
    panNode.orientationX.value = spatialParams.orientation[0]
    panNode.orientationY.value = spatialParams.orientation[1]
    panNode.orientationZ.value = spatialParams.orientation[2]
  }

  createOutputNodes(src: string): { scriptProcessor: ScriptProcessorNode; panNode: PannerNode } {
    const scriptProcessor = this.createScriptOutputFor(src)
    const panNode = this.context.createPanner()
    panNode.coneInnerAngle = 140
    panNode.coneOuterAngle = 360
    panNode.coneOuterGain = 0.8
    panNode.maxDistance = this.options.maxDistance ?? 40
    panNode.refDistance = this.options.refDistance ?? 2
    scriptProcessor.connect(panNode)
    panNode.connect(this.context.destination)

    return { scriptProcessor, panNode }
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
    this.inputProcessor.connect(this.context.destination)
  }

  pause() {
    this.inputProcessor.disconnect(this.context.destination)
  }

  private createEncodeStream() {
    const encodeStream = this.voiceChatWorkerMain.getOrCreateEncodeStream(this.selfId, this.sampleRate)
    encodeStream.addAudioEncodedListener((data) => this.channel.send(data))

    this.inputProcessor.onaudioprocess = async function (e) {
      const buffer = e.inputBuffer
      encodeStream.encode(buffer.getChannelData(0))
    }
  }

  private setVoiceRelativePosition(src: string, spatialParams: VoiceSpatialParams) {
    this.outputs[src].spatialParams = spatialParams
    this.updatePannerNodeParameters(src)
  }
}
