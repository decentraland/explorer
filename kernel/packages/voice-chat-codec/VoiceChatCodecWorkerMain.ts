import { ResponseTopic, RequestTopic, VoiceChatWorkerRequest } from './types'

const workerUrl = 'voice-chat-codec/worker.js'

type EncodeListener = (encoded: Uint8Array) => any
type DecodeListener = (samples: Float32Array) => any

type EncodeStream = {
  encode(samples: Float32Array): void
  addAudioEncodedListener(listener: EncodeListener): void
}

type DecodeStream = {
  decode(encoded: Uint8Array): void
  addAudioDecodedListener(listener: DecodeListener): void
}

export class VoiceChatCodecWorkerMain {
  private requestId: number = 0
  private worker: Worker

  private encodeListeners: Record<string, EncodeListener[]> = {}
  private decodeListeners: Record<string, DecodeListener[]> = {}

  public readonly encodeStreams: Record<string, EncodeStream> = {}
  public readonly decodeStreams: Record<string, DecodeStream> = {}

  constructor() {
    this.worker = new Worker(workerUrl, { name: 'VoiceChatCodecWorker' })
    this.worker.onmessage = (ev) => {
      if (ev.data.topic === ResponseTopic.ENCODE) {
        this.encodeListeners[ev.data.streamId]?.forEach((listener) => listener(ev.data.encoded))
      } else if (ev.data.topic === ResponseTopic.DECODE) {
        this.decodeListeners[ev.data.streamId]?.forEach((listener) => listener(ev.data.samples))
      } else {
        console.warn('Unknown message topic received from worker', ev)
      }
    }
  }

  private generateId() {
    return this.requestId++
  }

  getOrCreateEncodeStream(streamId: string, sampleRate: number): EncodeStream {
    return (this.encodeStreams[streamId] = this.encodeStreams[streamId] || {
      encode: (samples) => {
        this.sendRequestToWorker({ topic: RequestTopic.ENCODE, sampleRate: sampleRate, samples, streamId })
      },
      addAudioEncodedListener: (listener) => {
        this.addAudioEncodedListener(streamId, listener)
      }
    })
  }

  getOrCreateDecodeStream(streamId: string, sampleRate: number): DecodeStream {
    return (this.decodeStreams[streamId] = this.decodeStreams[streamId] || {
      decode: (encoded) => {
        this.sendRequestToWorker({ topic: RequestTopic.DECODE, sampleRate: sampleRate, encoded, streamId })
      },
      addAudioDecodedListener: (listener) => {
        this.addAudioDecodedListener(streamId, listener)
      }
    })
  }

  private addListenerFor<T>(streamId: string, listeners: Record<string, T[]>, listener: T) {
    if (!listeners[streamId]) {
      listeners[streamId] = []
    }

    listeners[streamId].push(listener)
  }

  addAudioEncodedListener(streamId: string, listener: EncodeListener) {
    this.addListenerFor(streamId, this.encodeListeners, listener)
  }

  addAudioDecodedListener(streamId: string, listener: DecodeListener) {
    this.addListenerFor(streamId, this.decodeListeners, listener)
  }

  destroyEncodeStream(streamId: string) {
    delete this.encodeStreams[streamId]
    this.sendRequestToWorker({ topic: RequestTopic.DESTROY_ENCODER, streamId })
  }

  destroyDecodeStream(streamId: string) {
    delete this.decodeStreams[streamId]
    this.sendRequestToWorker({ topic: RequestTopic.DESTROY_DECODER, streamId })
  }

  private sendRequestToWorker(message: VoiceChatWorkerRequest) {
    const id = this.generateId()
    message.id = id
    this.worker.postMessage(message)
  }
}
