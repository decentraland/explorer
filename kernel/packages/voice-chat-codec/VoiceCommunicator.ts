import { VoiceChatCodecWorkerMain, EncodeStream } from './VoiceChatCodecWorkerMain'
import { OrderedRingBuffer } from 'atomicHelpers/OrderedRingBuffer'
import { defer } from 'atomicHelpers/defer'
import defaultLogger from 'shared/logger'
import { OPUS_BITS_PER_SECOND, OPUS_FRAME_SIZE_MS, OUTPUT_NODE_BUFFER_SIZE } from './constants'

export type AudioCommunicatorChannel = {
  send(data: Uint8Array): any
}

export type StreamPlayingListener = (streamId: string, playing: boolean) => any
export type StreamRecordingListener = (recording: boolean) => any

type VoiceOutput = {
  encodedBuffer: OrderedRingBuffer<Uint8Array>
  decodedBuffer: OrderedRingBuffer<Float32Array>
  scriptProcessor: ScriptProcessorNode
  panNode: PannerNode
  spatialParams: VoiceSpatialParams
  playing: boolean
  lastUpdateTime: number
}

type VoiceInput = {
  encodeInputProcessor: ScriptProcessorNode
  inputStream: MediaStreamAudioSourceNode
  recordingContext: AudioContext
  encodeStream: EncodeStream
}

export type VoiceCommunicatorOptions = {
  sampleRate?: number
  channelBufferSize?: number
  maxDistance?: number
  refDistance?: number
  initialListenerParams?: VoiceSpatialParams
  panningModel?: PanningModelType
  distanceModel?: DistanceModelType
}

export type VoiceSpatialParams = {
  position: [number, number, number]
  orientation: [number, number, number]
}

export class VoiceCommunicator {
  private context: AudioContext
  private outputGainNode: GainNode
  private input?: VoiceInput
  private voiceChatWorkerMain: VoiceChatCodecWorkerMain
  private outputs: Record<string, VoiceOutput> = {}

  private streamPlayingListeners: StreamPlayingListener[] = []
  private streamRecordingListeners: StreamRecordingListener[] = []

  private readonly sampleRate: number
  private readonly channelBufferSize: number
  private readonly outputExpireTime = 60 * 1000

  constructor(
    private selfId: string,
    private channel: AudioCommunicatorChannel,
    private options: VoiceCommunicatorOptions
  ) {
    this.sampleRate = this.options.sampleRate ?? 24000
    this.channelBufferSize = this.options.channelBufferSize ?? 2.0

    this.context = new AudioContext({ sampleRate: this.sampleRate })
    this.outputGainNode = this.context.createGain()
    this.outputGainNode.connect(this.context.destination)

    if (this.options.initialListenerParams) {
      this.setListenerSpatialParams(this.options.initialListenerParams)
    }

    this.voiceChatWorkerMain = new VoiceChatCodecWorkerMain()

    this.startOutputsExpiration()
  }

  public setSelfId(selfId: string) {
    this.voiceChatWorkerMain.destroyEncodeStream(this.selfId)
    this.selfId = selfId
    if (this.input) {
      this.input.encodeStream = this.createInputEncodeStream(
        this.input.recordingContext,
        this.input.encodeInputProcessor
      )
    }
  }

  public addStreamPlayingListener(listener: StreamPlayingListener) {
    this.streamPlayingListeners.push(listener)
  }

  public addStreamRecordingListener(listener: StreamRecordingListener) {
    this.streamRecordingListeners.push(listener)
  }

  public hasInput() {
    return !!this.input
  }

  async playEncodedAudio(src: string, relativePosition: VoiceSpatialParams, encoded: Uint8Array, time: number) {
    if (!this.outputs[src]) {
      this.createOutput(src, relativePosition)
    } else {
      this.outputs[src].lastUpdateTime = Date.now()
      this.setVoiceRelativePosition(src, relativePosition)
    }

    this.outputs[src].encodedBuffer.write(encoded, time)
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

  setVolume(value: number) {
    this.outputGainNode.gain.value = value
  }

  createOutputNodes(src: string): { scriptProcessor: ScriptProcessorNode; panNode: PannerNode } {
    const scriptProcessor = this.createScriptOutputFor(src)
    const panNode = this.context.createPanner()
    panNode.coneInnerAngle = 180
    panNode.coneOuterAngle = 360
    panNode.coneOuterGain = 0.9
    panNode.maxDistance = this.options.maxDistance ?? 10000
    panNode.refDistance = this.options.refDistance ?? 5
    panNode.panningModel = this.options.panningModel ?? 'equalpower'
    panNode.distanceModel = this.options.distanceModel ?? 'inverse'
    panNode.rolloffFactor = 1.0
    scriptProcessor.connect(panNode)
    panNode.connect(this.outputGainNode)

    return { scriptProcessor, panNode }
  }

  createScriptOutputFor(src: string) {
    const bufferSize = OUTPUT_NODE_BUFFER_SIZE
    const processor = this.context.createScriptProcessor(bufferSize, 0, 1)
    processor.onaudioprocess = (ev) => {
      const data = ev.outputBuffer.getChannelData(0)

      data.fill(0)
      if (this.outputs[src]) {
        const wasPlaying = this.outputs[src].playing
        const minReadCount = wasPlaying ? 0 : OUTPUT_NODE_BUFFER_SIZE - 1
        if (this.outputs[src].decodedBuffer.readAvailableCount() > minReadCount) {
          data.set(this.outputs[src].decodedBuffer.read(data.length))
          if (!wasPlaying) {
            this.changePlayingStatus(src, true)
          }
        } else {
          if (wasPlaying) {
            this.changePlayingStatus(src, false)
            this.outputs[src].decodedBuffer.read() // Emptying buffer
          }
        }
      }
    }

    return processor
  }

  changePlayingStatus(streamId: string, playing: boolean) {
    this.outputs[streamId].playing = playing
    this.outputs[streamId].lastUpdateTime = Date.now()
    // Listeners could be long running, so we defer the execution of them
    defer(() => {
      this.streamPlayingListeners.forEach((listener) => listener(streamId, playing))
    })
  }

  setInputStream(stream: MediaStream) {
    if (this.input) {
      this.voiceChatWorkerMain.destroyEncodeStream(this.selfId)
      if (this.input.recordingContext !== this.context) {
        this.input.recordingContext.close().catch((e) => defaultLogger.error('Error closing recording context', e))
      }
    }

    try {
      this.input = this.createInputFor(stream, this.context)
    } catch (e) {
      // If this fails, then it most likely it is because the sample rate of the stream is incompatible with the context's, so we create a special context for recording
      if (e.message.includes('sample-rate is currently not supported')) {
        const recordingContext = new AudioContext()
        this.input = this.createInputFor(stream, recordingContext)
      } else {
        throw e
      }
    }
  }

  start() {
    if (this.input) {
      this.input.encodeInputProcessor.connect(this.input.recordingContext.destination)
      this.input.inputStream.connect(this.input.encodeInputProcessor)
      this.notifyRecording(true)
    } else {
      this.notifyRecording(false)
    }
  }

  pause() {
    try {
      this.input?.inputStream.disconnect(this.input.encodeInputProcessor)
    } catch (e) {
      // Ignored. This will fail if it was already disconnected
    }

    try {
      this.input?.encodeInputProcessor.disconnect(this.input.recordingContext.destination)
    } catch (e) {
      // Ignored. This will fail if it was already disconnected
    }

    this.notifyRecording(false)
  }

  private createOutput(src: string, relativePosition: VoiceSpatialParams) {
    const nodes = this.createOutputNodes(src)
    this.outputs[src] = {
      encodedBuffer: new OrderedRingBuffer(Math.floor((OPUS_BITS_PER_SECOND * this.channelBufferSize) / 8), Uint8Array),
      decodedBuffer: new OrderedRingBuffer(Math.floor(this.channelBufferSize * this.sampleRate), Float32Array),
      playing: false,
      spatialParams: relativePosition,
      lastUpdateTime: Date.now(),
      ...nodes
    }

    const readEncodedBufferLoop = async () => {
      if (this.outputs[src]) {
        const amountToRead = Math.floor(((OUTPUT_NODE_BUFFER_SIZE / this.sampleRate) * 1000 * 1.2) / OPUS_FRAME_SIZE_MS)

        const frames = await this.outputs[src].encodedBuffer.blockAndReadChunks(
          amountToRead,
          amountToRead * OPUS_FRAME_SIZE_MS
        )

        if (frames.length > 0) {
          let stream = this.voiceChatWorkerMain.decodeStreams[src]

          if (!stream) {
            stream = this.voiceChatWorkerMain.getOrCreateDecodeStream(src, this.sampleRate)

            stream.addAudioDecodedListener((samples) => {
              const now = Date.now()
              this.outputs[src].lastUpdateTime = now
              this.outputs[src].decodedBuffer.write(samples, now)
            })
          }

          frames.forEach((it) => stream.decode(it))
        }

        await readEncodedBufferLoop()
      }
    }

    readEncodedBufferLoop().catch((e) => defaultLogger.log('Error while reading encoded buffer of ' + src, e))
  }

  private createInputFor(stream: MediaStream, context: AudioContext) {
    const streamSource = context.createMediaStreamSource(stream)
    const inputProcessor = context.createScriptProcessor(4096, 1, 1)
    return {
      recordingContext: context,
      encodeStream: this.createInputEncodeStream(context, inputProcessor),
      encodeInputProcessor: inputProcessor,
      inputStream: streamSource
    }
  }

  private createInputEncodeStream(recordingContext: AudioContext, encodeInputProcessor: ScriptProcessorNode) {
    const encodeStream = this.voiceChatWorkerMain.getOrCreateEncodeStream(
      this.selfId,
      this.sampleRate,
      recordingContext.sampleRate
    )

    encodeStream.addAudioEncodedListener((data) => this.channel.send(data))

    encodeInputProcessor.onaudioprocess = async (e) => {
      const buffer = e.inputBuffer
      encodeStream.encode(buffer.getChannelData(0))
    }

    return encodeStream
  }

  private setVoiceRelativePosition(src: string, spatialParams: VoiceSpatialParams) {
    this.outputs[src].spatialParams = spatialParams
    this.updatePannerNodeParameters(src)
  }

  private notifyRecording(recording: boolean) {
    this.streamRecordingListeners.forEach((listener) => listener(recording))
  }

  private startOutputsExpiration() {
    const expireOutputs = () => {
      Object.keys(this.outputs).forEach((outputId) => {
        const output = this.outputs[outputId]
        if (Date.now() - output.lastUpdateTime > this.outputExpireTime) {
          this.destroyOutput(outputId)
        }
      })

      setTimeout(expireOutputs, 2000)
    }

    setTimeout(expireOutputs, 0)
  }

  private destroyOutput(outputId: string) {
    try {
      this.outputs[outputId].panNode.disconnect(this.context.destination)
    } catch (e) {
      // Ignored. This may fail if the node wasn't connected yet
    }

    this.voiceChatWorkerMain.destroyDecodeStream(outputId)

    delete this.outputs[outputId]
  }
}
