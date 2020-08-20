declare const Worker: any
declare const DCL: any

const gifProcessorWorkerRaw = require('raw-loader!../../static/gif-processor/worker.js')
const gifProcessorWorkerUrl = URL.createObjectURL(new Blob([gifProcessorWorkerRaw]))
const worker = new Worker(gifProcessorWorkerUrl, { name: 'gifProcessorWorker' })

/**
 *
 * Class that triggers GIF processing in a worker and dispatches the generated texture pointers to Unity
 *
 */
export class GIFProcessor {
  gameInstance: any
  GLctx: any
  unityInterface: any
  isWebGL1: boolean = false

  constructor (gameInstance: any, unityInterface: any, isWebGL1: boolean) {
    this.gameInstance = gameInstance
    this.GLctx = this.gameInstance.Module.ctx
    this.unityInterface = unityInterface
    this.isWebGL1 = isWebGL1

    worker.onmessage = (e: any) => {
      if (e.data.frames.length <= 0) return

      const textures = new Array()
      const texIDs = new Array()
      const frames = e.data.arrayBufferFrames
      const width = e.data.width
      const height = e.data.height
      const frameDelays = e.data.delays
      const sceneId = e.data.sceneId
      const componentId = e.data.componentId

      // Generate all the GIF textures
      for (let index = 0; index < e.data.frames.length; index++) {
        const ptr: GLuint = this.gameInstance.Module._malloc(4)
        const tex = this.GenerateTexture(ptr)

        textures.push(tex)
        texIDs.push(tex.name)

        // print current image data onto current tex
        const frameImageData = new ImageData(new Uint8ClampedArray(frames[index]), width, height)
        this.UpdateGIFTex(frameImageData, tex.name)
      }

      this.unityInterface.SendGIFPointers(sceneId, componentId, width, height, texIDs, frameDelays)
    }
  }

  /**
   *
   * Tells the worker to process a GIF
   *
   */
  ProcessGIF(data: { imageSource: string, sceneId: string, componentId: string }) {
    worker.postMessage({ src: data.imageSource, sceneId: data.sceneId, componentId: data.componentId })
  }

  /**
   *
   * Based on WebGL compiled "glGenTextures", created a WebGL texture and returns it
   *
   */
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

  /**
   *
   * "Prints" an ImageData into an already existent WebGL texture
   *
   */
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
