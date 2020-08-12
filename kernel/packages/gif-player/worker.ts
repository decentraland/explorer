import defaultLogger from 'shared/logger'
declare const self: any

const fastgif = require('fastgif/fastgif.js')
const gifDecoder = new fastgif.Decoder()

{
  self.onmessage = async (e: any) => {
    defaultLogger.log('pravs - GIF WORKER - CALLED...', e)

    defaultLogger.log('pravs - GIF WORKER - trying to fetch...', e.data.src)
    const imageFetch = fetch(e.data.src)

    const response = await imageFetch
    const buffer = await response.arrayBuffer()

    const frames = await gifDecoder.decode(buffer) // an array of {imageData: ImageData, delay: number}

    defaultLogger.log('pravs - GIF WORKER - CALLING MAIN...')

    // TODO: Find a way to send ImageData instances as a Transferable (to avoid cloning)
    self.postMessage(frames)
  }
}
