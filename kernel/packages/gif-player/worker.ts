import defaultLogger from 'shared/logger'
declare const self: any

import { parseGIF, decompressFrames } from 'gifuct-js'

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

    const arrayBufferFrames = new Array()
    const frameDelays = new Array()
    for (const key in decompressedFrames) {
      arrayBufferFrames.push(decompressedFrames[key].patch.buffer)
      frameDelays.push(decompressedFrames[key].delay)
    }

    // Passing ArrayBuffer made from the frames Uint8ClampedArray as transferable
    self.postMessage({
      frames: arrayBufferFrames,
      width: decompressedFrames[0].dims.width,
      height: decompressedFrames[0].dims.height,
      delays: frameDelays,
      sceneId: e.data.sceneId,
      componentId: e.data.componentId
    }, arrayBufferFrames)
  }
}
