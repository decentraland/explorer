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
    EnqueuePayload(e)
  }

  function EnqueuePayload(e: any) {
    payloads.push(e)
    if (payloads.length === 1) {
      const promise = ConsumePayload()
      promise.catch((error) => defaultLogger.log(error))
    }
  }

  async function ConsumePayload() {
    while (payloads.length > 0) {
      await DownloadAndProcessGIF(payloads[0])
      payloads.splice(0, 1)
    }
  }

  async function DownloadAndProcessGIF(e: any) {
    const imageFetch = fetch(e.data.src)
    const response = await imageFetch
    const buffer = await response.arrayBuffer()
    const parsedGif = await parseGIF(buffer)
    const decompressedFrames = decompressFrames(parsedGif, true)

    frameImageData = undefined
    gifCanvas.width = decompressedFrames[0].dims.width
    gifCanvas.height = decompressedFrames[0].dims.height
    gifCanvasCtx?.scale(1, -1) // We have to flip it vertically or it's rendered upside down

    const frameDelays = new Array()
    const framesAsArrayBuffer = new Array()
    for (const key in decompressedFrames) {
      frameDelays.push(decompressedFrames[key].delay)
      framesAsArrayBuffer.push(GenerateFinalImageData(decompressedFrames[key]).data.buffer)
    }

    self.postMessage({
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

      gifCanvasCtx?.drawImage(gifPatchCanvas, frame.dims.left, -(gifCanvas.height - frame.dims.top))  // We have to flip it vertically or it's rendered upside down
    }

    return gifCanvasCtx?.getImageData(0, 0, gifCanvas.width , gifCanvas.height)
  }
}
