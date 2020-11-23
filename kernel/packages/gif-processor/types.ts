export type GifAsset = {
  id: string
  url: string
  textures: { name: any; imageData: ImageData }[]
  delays: number[]
  width: number
  height: number
  pending: boolean
}

export type WorkerMessage = {
  data: WorkerMessageData
}

export type WorkerMessageData = {
  arrayBufferFrames: Array<ArrayBufferLike> | undefined
  width: number
  height: number
  delays: number[]
  url: string
  id: string
}

export type ProcessorMessage = {
  data: ProcessorMessageData
}

export type ProcessorMessageData = {
  url: string
  id: string
}
