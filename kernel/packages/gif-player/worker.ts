import defaultLogger from 'shared/logger'
declare const self: any

const fastgif = require('fastgif/fastgif.js')
const gifDecoder = new fastgif.Decoder()

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

    const frames = await gifDecoder.decode(buffer) // an array of {imageData: ImageData, delay: number}

    // TODO: Find a way to send ImageData instances as a Transferable (to avoid cloning)
    self.postMessage({ frames: frames, sceneId: e.data.sceneId, componentId: e.data.componentId })
  }
}
