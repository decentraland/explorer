import defaultLogger from 'shared/logger'
declare const self: any

import { parseGIF, decompressFrames } from 'gifuct-js'

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

    const frameDelays = new Array()
    for (const key in decompressedFrames) {
      frameDelays.push(decompressedFrames[key].delay)
    }

    self.postMessage({
      frames: decompressedFrames,
      delays: frameDelays,
      sceneId: e.data.sceneId,
      componentId: e.data.componentId
    })
  }
}
