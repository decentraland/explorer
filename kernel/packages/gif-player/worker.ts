import defaultLogger from 'shared/logger'
import { parseGIF, decompressFrames } from 'gifuct-js'

declare const self: any

const gifCanvas = new OffscreenCanvas(1,1)
const gifCanvasCtx = gifCanvas.getContext('2d')
const gifPatchCanvas = new OffscreenCanvas(1,1)
const gifPatchCanvasCtx = gifPatchCanvas.getContext('2d')
let frameImageData: any = undefined

{
  let payloads: any[] = new Array()

  self.onmessage = (e: any) => {
    defaultLogger.log("pravs - WORKER - Entrypoint called", e)
    EnqueuePayload(e)
  }

  function EnqueuePayload(e: any) {
    defaultLogger.log("pravs - WORKER - Enqueue Payload", e)
    payloads.push(e)
    if (payloads.length === 1) {
      const promise = ConsumePayload()
      promise.catch((error) => defaultLogger.log(error))
    }
  }

  async function ConsumePayload() {
    while (payloads.length > 0) {
      defaultLogger.log("pravs - WORKER - Consuming first payload", payloads)
      await DownloadAndProcessGIF(payloads[0])
      payloads.splice(0, 1)
    }
  }

  async function DownloadAndProcessGIF(e: any) {
    defaultLogger.log("pravs - WORKER - DownloadAndProcessGIF")
    const imageFetch = fetch(e.data.src)
    const response = await imageFetch

    const buffer = await response.arrayBuffer()

    const parsedGif = await parseGIF(buffer)
    const decompressedFrames = decompressFrames(parsedGif, true)
    defaultLogger.log("pravs - WORKER PROCESSED GIF:", decompressedFrames)

    frameImageData = undefined
    gifCanvas.width = decompressedFrames[0].dims.width
    gifCanvas.height = decompressedFrames[0].dims.height

    const frameDelays = new Array()
    const framesAsArrayBuffer = new Array()
    for (const key in decompressedFrames) {
      frameDelays.push(decompressedFrames[key].delay)
      framesAsArrayBuffer.push(GenerateFinalImageData(decompressedFrames[key]).data.buffer)
    }

    self.postMessage({
      frames: decompressedFrames,
      arrayBufferFrames: framesAsArrayBuffer,
      width: decompressedFrames[0].dims.width,
      height: decompressedFrames[0].dims.height,
      delays: frameDelays,
      sceneId: e.data.sceneId,
      componentId: e.data.componentId
    }, framesAsArrayBuffer)
  }

  function GenerateFinalImageData(frame: any): any {
    if (!frameImageData || frame.dims.width !== frameImageData.width || frame.dims.height !== frameImageData.height) {
      gifPatchCanvas.width = frame.dims.width
      gifPatchCanvas.height = frame.dims.height

      frameImageData = gifPatchCanvasCtx?.createImageData(frame.dims.width, frame.dims.height)
    }

    if (frameImageData) {
      frameImageData.data.set(frame.patch)
      gifPatchCanvasCtx?.putImageData(frameImageData, 0, 0)

      gifCanvasCtx?.drawImage(gifPatchCanvas, frame.dims.left, frame.dims.top)
    }

    return gifCanvasCtx?.getImageData(0, 0, gifCanvas.width , gifCanvas.height)
  }
}
