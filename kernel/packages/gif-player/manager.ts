import defaultLogger from 'shared/logger'

declare const Worker: any
declare const DCL: any

const gifPlayerWorkerRaw = require('raw-loader!../../static/gif-player/worker.js')
const gifPlayerWorkerUrl = URL.createObjectURL(new Blob([gifPlayerWorkerRaw]))
const worker = new Worker(gifPlayerWorkerUrl, { name: 'gifPlayerWorker' })

class GIF {
  frames: any[]
  width: number = 0
  height: number = 0
  frameDelays: number[]
  currentFrameIndex: number = 0
  texture: any

  constructor(frames: any[], width: number, height: number, frameDelays: number[], texture: any) {
    this.frames = frames
    this.texture = texture
    this.frameDelays = frameDelays

    for (const key in frames) {
      const arreyBufferFrame = frames[key]

      const frameImagedata = new ImageData(new Uint8ClampedArray(arreyBufferFrame), width, height)

      frames[key].imageData = frameImagedata
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
      if (e.data.frames.length <= 0) return

      const frames = e.data.frames
      const width = e.data.width
      const height = e.data.height
      const frameDelays = e.data.delays
      const sceneId = e.data.sceneId
      const componentId = e.data.componentId

      // Generate texture that will be used for displaying the GIF frames
      const ptr: GLuint = this.gameInstance.Module._malloc(4)
      const tex = this.GenerateTexture(ptr)

      this.gifs[this.GenerateGIFKey(sceneId, componentId)] = new GIF(frames, width, height, frameDelays, tex)

      this.unityInterface.SendGIFPointer(sceneId, componentId, width, height, tex.name)
    }
  }

  PlayGIF(data: { imageSource: string, sceneId: string, componentId: string }) {
    worker.postMessage({ src: data.imageSource, sceneId: data.sceneId, componentId: data.componentId })
  }

  // TODO: If we make sure Unity tracks more than 1 component using the same GIF and calls the StopGIF()
  // when there are no remaining references, then we can just send the src and use that as the key
  StopGIF(data: { sceneId: string, componentId: string }) {
    const gifId = this.GenerateGIFKey(data.sceneId, data.componentId)

    this.GLctx.deleteTexture(DCL.GL.textures[this.gifs[gifId].texture.name])

    delete this.gifs[gifId]
  }

  GenerateGIFKey(sceneId: string, componentId: string): string {
    return sceneId.concat(componentId)
  }

  async PlayGIFPromise() {
    while (true) {
      const delay = 16 // 60FPS
      const previousTime = new Date().getTime()
      await new Promise((resolve) => window.setTimeout(resolve, delay))

      const timeDiff = new Date().getTime() - previousTime

      for (const key in this.gifs) {
        const gif = this.gifs[key]

        const enoughTimePassedToChangeFrame = timeDiff >= gif.frameDelays[gif.currentFrameIndex]
        if (!enoughTimePassedToChangeFrame) continue

        this.UpdateGIFTex(gif.frames[gif.currentFrameIndex].imageData, gif.texture.name)

        if (++gif.currentFrameIndex === gif.frames.length) {
          gif.currentFrameIndex = 0
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
