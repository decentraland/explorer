import defaultLogger from 'shared/logger'
declare const self: any

import { parseGIF, decompressFrames } from 'gifuct-js'

{
  let payloads: any[] = new Array()

  self.onmessage = (e: any) => {
    defaultLogger.log('pravs - GIF WORKER - trying to fetch...', e.data.src)

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
    defaultLogger.log('pravs - GIF WORKER - fetched...', response)

    const buffer = await response.arrayBuffer()

    const parsedGif = await parseGIF(buffer)
    const decompressedFrames = decompressFrames(parsedGif, true)

    // TODO: Find a way to send data as a Transferable (to avoid cloning)
    self.postMessage({ frames: decompressedFrames, sceneId: e.data.sceneId, componentId: e.data.componentId })
  }
}
