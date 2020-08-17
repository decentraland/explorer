import defaultLogger from 'shared/logger'

declare const Worker: any
declare const DCL: any

const gifPlayerWorkerRaw = require('raw-loader!../../static/gif-player/worker.js')
const gifPlayerWorkerUrl = URL.createObjectURL(new Blob([gifPlayerWorkerRaw]))
const offscreenCanvas = new OffscreenCanvas(1,1)
const offscreenCanvasCtx = offscreenCanvas.getContext('2d')

// tslint:disable-next-line
const worker = new Worker(gifPlayerWorkerUrl, { name: 'gifPlayerWorker' })

class GIF {
  frames: any[]
  currentFrame: number = 0
  texture: any

  constructor(frames: any[], texture: any) {
    this.frames = frames
    this.texture = texture

    for (const key in frames) {
      const frame = frames[key]
      const frameImagedata = offscreenCanvasCtx?.createImageData(frame.dims.width, frame.dims.height)
      frameImagedata?.data.set(frame.patch)

      frame.imageData = frameImagedata
    }
  }
}

export class GIFPlayer {
  gameInstance: any
  GLctx: any
  unityInterface: any
  isWebGL1: boolean = false
  playingPromise: Promise<void>
  gifs: { [id: string]: GIF } = {}

  constructor (gameInstance: any, unityInterface: any, isWebGL1: boolean) {
    this.gameInstance = gameInstance
    this.GLctx = this.gameInstance.Module.ctx
    this.unityInterface = unityInterface
    this.isWebGL1 = isWebGL1

    this.playingPromise = this.PlayGIFPromise()
    this.playingPromise.catch((error) => defaultLogger.log(error))

    worker.onmessage = (e: any) => {
      const frames = e.data.frames
      const sceneId = e.data.sceneId
      const componentId = e.data.componentId

      defaultLogger.log('pravs - GIFPlayer - got back from WORKER', e)
      if (frames.length <= 0) return

      // Generate texture that will be used for displaying the GIF frames
      const ptr: GLuint = this.gameInstance.Module._malloc(4)
      const tex = this.GenerateTexture(ptr)

      defaultLogger.log('pravs - GIFPlayer - adding GIF to the collection, for ' + this.GenerateGIFKey(sceneId, componentId))
      this.gifs[this.GenerateGIFKey(sceneId, componentId)] = new GIF(frames, tex)

      this.unityInterface.SendGIFPointer(sceneId, componentId, frames[0].dims.width, frames[0].dims.height, tex.name)
    }
  }

  PlayGIF(data: { imageSource: string, sceneId: string, componentId: string }) {
    // We process the GIF in the worker and get an array of {imageData: ImageData, delay: number} in e.data
    worker.postMessage({ src: data.imageSource, sceneId: data.sceneId, componentId: data.componentId })
  }

  // TODO: If we make sure Unity tracks more than 1 component using the same GIF and calls the StopGIF()
  // when there are no remaining references, then we can just send the src and use that as the key
  StopGIF(data: { sceneId: string, componentId: string }) {
    // TODO: destroy WebGL tex

    delete this.gifs[this.GenerateGIFKey(data.sceneId, data.componentId)]
  }

  GenerateGIFKey(sceneId: string, componentId: string): string {
    return sceneId.concat(componentId)
  }

  async PlayGIFPromise() {
    while (true) {
      // TODO: frames[frameIndex].delay is always 0 ???
      let delay = 64 // 15FPS
      await new Promise((resolve) => window.setTimeout(resolve, delay))

      // tslint:disable-next-line
      // defaultLogger.log('pravs - GIFPlayer - playing ' + Object.keys(this.gifs).length + ' GIFs')

      for (const key in this.gifs) {
        const gif = this.gifs[key]

        this.UpdateGIFTex(gif.frames[gif.currentFrame].imageData, gif.texture.name)

        if (++gif.currentFrame === gif.frames.length) {
          gif.currentFrame = 0
        }
      }
    }
  }

  // Based on WebGL compiled "glGenTextures"
  GenerateTexture(ptr: any): any {
    let GLctx = this.gameInstance.Module.ctx
    let texture = GLctx.createTexture()

    if (!texture) {
      DCL.GL.recordError(1282)
      this.gameInstance.Module.HEAP32[ptr + 4 >> 2] = 0
      return
    }

    let id = DCL.GL.getNewId(DCL.GL.textures)
    texture.name = id
    DCL.GL.textures[id] = texture
    this.gameInstance.Module.HEAP32[ptr >> 2] = id

    return texture
  }

  UpdateGIFTex(image: any, texId: any) {
    this.GLctx.bindTexture(this.GLctx.TEXTURE_2D, DCL.GL.textures[texId])

    if (this.isWebGL1) {
      this.GLctx.texImage2D(
        this.GLctx.TEXTURE_2D,
        0,
        this.GLctx.RGBA,
        this.GLctx.RGBA,
        this.GLctx.UNSIGNED_BYTE,
        image
      )
    } else {
      this.GLctx.texImage2D(
        this.GLctx.TEXTURE_2D,
        0,
        this.GLctx.RGBA,
        image.width,
        image.height,
        0,
        this.GLctx.RGBA,
        this.GLctx.UNSIGNED_BYTE,
        image
      )
    }
  }
}
