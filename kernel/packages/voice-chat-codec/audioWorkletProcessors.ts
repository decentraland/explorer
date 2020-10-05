import { OPUS_SAMPLES_PER_FRAME } from './constants'
import { WorkletRequestTopic } from './types'

export interface AudioWorkletProcessor {
  readonly port: MessagePort
  process(inputs: Float32Array[][], outputs: Float32Array[][], parameters: Record<string, Float32Array>): boolean
}

declare var AudioWorkletProcessor: {
  prototype: AudioWorkletProcessor
  new (options?: AudioWorkletNodeOptions): AudioWorkletProcessor
}

declare function registerProcessor(
  name: string,
  processorCtor: (new (options?: AudioWorkletNodeOptions) => AudioWorkletProcessor) & {
    parameterDescriptors?: AudioParamDescriptor[]
  }
): void

enum InputProcessorStatus {
  RECORDING,
  PAUSE_REQUESTED,
  PAUSED
}

class InputProcessor extends AudioWorkletProcessor {
  status: InputProcessorStatus = InputProcessorStatus.PAUSED
  inputSamplesCount: number = 0

  constructor(...args: any[]) {
    super(...args)

    this.port.onmessage = (e) => {
      if (e.data.topic === WorkletRequestTopic.PAUSE) {
        this.status = InputProcessorStatus.PAUSE_REQUESTED
      }

      if (e.data.topic === WorkletRequestTopic.RESUME) {
        this.status = InputProcessorStatus.RECORDING
        this.notify(WorkletRequestTopic.ON_RECORDING)
      }
    }
  }

  process(inputs: Float32Array[][], outputs: Float32Array[][], parameters: Record<string, Float32Array>) {
    if (this.status === InputProcessorStatus.PAUSED) return true
    let data = inputs[0][0]

    if (this.status === InputProcessorStatus.PAUSE_REQUESTED) {
      // We try to use as many samples as we can that would complete some frames
      const samplesToUse =
        Math.floor(data.length / OPUS_SAMPLES_PER_FRAME) * OPUS_SAMPLES_PER_FRAME +
        OPUS_SAMPLES_PER_FRAME -
        (this.inputSamplesCount % OPUS_SAMPLES_PER_FRAME)
      data = data.slice(0, samplesToUse)

      this.status = InputProcessorStatus.PAUSED
      this.notify(WorkletRequestTopic.ON_PAUSED)
    }

    this.sendDataToEncode(data)
    this.inputSamplesCount += data.length

    return true
  }

  notify(notification: WorkletRequestTopic) {
    this.port.postMessage({ topic: notification })
  }

  private sendDataToEncode(data: Float32Array) {
    this.port.postMessage({ topic: WorkletRequestTopic.ENCODE, samples: data }, [data.buffer])
  }
}

registerProcessor('inputProcessor', InputProcessor)
