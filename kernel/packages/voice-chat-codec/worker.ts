import { VoiceChatWorkerResponse, RequestTopic, ResponseTopic } from './types'
declare var self: WorkerGlobalScope & any

declare function postMessage(message: any): void

declare const libopus: any

self.LIBOPUS_WASM_URL = 'libopus.wasm'

importScripts('libopus.wasm.js')

type CodecWorklet = {
  working: boolean
}

type EncoderWorklet = {
  encoder: any
} & CodecWorklet

type DecoderWorklet = {
  decoder: any
} & CodecWorklet

function getSampleRate(e: MessageEvent) {
  return e.data.sampleRate ? e.data.sampleRate : 48000
}

const encoderWorklets: Record<string, EncoderWorklet> = {}
const decoderWorklets: Record<string, DecoderWorklet> = {}

function startWorklet<T extends CodecWorklet, O extends Uint8Array | Float32Array>(
  streamId: string,
  worklet: T,
  outputFunction: (worklet: T) => O,
  messageBuilder: (output: O, streamId: string) => VoiceChatWorkerResponse
) {
  worklet.working = true

  function doWork() {
    let output = outputFunction(worklet)

    if (output) {
      postMessage(messageBuilder(output, streamId))
      setTimeout(doWork, 0)
    } else {
      worklet.working = false
    }
  }

  setTimeout(doWork, 0)
}

//Encoder(channels, samplerate, bitrate, frame_size, voice_optimization)

onmessage = function (e) {
  if (e.data.topic === RequestTopic.ENCODE) {
    const sampleRate = getSampleRate(e)
    const encoderWorklet = (encoderWorklets[e.data.streamId] = encoderWorklets[e.data.streamId] || {
      working: false,
      encoder: new libopus.Encoder(1, sampleRate, 24000, 20, true)
    })

    const samples = toInt16Samples(e.data.samples)

    encoderWorklet.encoder.input(samples)

    if (!encoderWorklet.working) {
      startWorklet(
        e.data.streamId,
        encoderWorklet,
        (worklet) => worklet.encoder.output(),
        (output, streamId) => ({ topic: ResponseTopic.ENCODE, streamId: streamId, encoded: output })
      )
    }
  }

  if (e.data.topic === RequestTopic.DECODE) {
    const sampleRate = getSampleRate(e)
    const decoderWorklet = (decoderWorklets[e.data.streamId] = decoderWorklets[e.data.streamId] || {
      working: false,
      decoder: new libopus.Decoder(1, sampleRate)
    })

    decoderWorklet.decoder.input(e.data.encoded)

    if (!decoderWorklet.working) {
      startWorklet(
        e.data.streamId,
        decoderWorklet,
        (worklet) => worklet.decoder.output(),
        (output, streamId) => ({
          topic: ResponseTopic.DECODE,
          streamId,
          samples: toFloat32Samples(output)
        })
      )
    }
  }

  if (e.data.topic === RequestTopic.DESTROY_DECODER) {
    const { streamId } = e.data
    decoderWorklets[streamId]?.decoder?.destroy()
  }

  if (e.data.topic === RequestTopic.DESTROY_ENCODER) {
    const { streamId } = e.data
    encoderWorklets[streamId]?.encoder?.destroy()
  }
}

function toInt16Samples(floatSamples: Float32Array) {
  return Int16Array.from(floatSamples, (floatSample) => {
    let val = Math.floor(32767 * floatSample)
    val = Math.min(32767, val)
    val = Math.max(-32768, val)
    return val
  })
}

function toFloat32Samples(intSamples: Int16Array) {
  return Float32Array.from(intSamples, (intSample) => {
    let floatValue = intSample >= 0 ? intSample / 32767 : intSample / 32768
    return Math.fround(floatValue)
  })
}
