import defaultLogger from 'shared/logger'

declare const Worker: any
declare const DCL: any

defaultLogger.log('pravs - GIFPLAYER - CREATING GIF WORKER')
const gifPlayerWorkerRaw = require('raw-loader!../../static/gif-player/worker.js')
const gifPlayerWorkerUrl = URL.createObjectURL(new Blob([gifPlayerWorkerRaw]))
const worker = new Worker(gifPlayerWorkerUrl, { name: 'gifPlayerWorker' })

export class GIFPlayer {
  gameInstance: any
  unityInterface: any

  constructor (gameInstance: any, unityInterface: any) {
    this.gameInstance = gameInstance
    this.unityInterface = unityInterface
  }

  PlayGIF(data: { imageSource: string, sceneId: string, componentId: string, isWebGL1: boolean }) {
    defaultLogger.log('pravs - GIFPLAYER - CALLING WORKER')

    // // Generate texture that will be used for displaying the GIF frames
    const ptr: GLuint = this.gameInstance.Module._malloc(4)
    const tex = this.GenerateTexture(ptr)

    worker.postMessage({ src: data.imageSource })

    worker.onmessage = (e: any) => {
      defaultLogger.log('pravs - GIFPLAYER - WORKER GOT BACK TO GIF PLAYER', e)

      this.unityInterface.SendGIFPointer(data.sceneId, data.componentId, e.data[0].imageData.width, e.data[0].imageData.height, tex.name)

      this.StartPlayGIFPromise(e.data, tex.name, data.isWebGL1)
    }
  }

  // Based on WebGL compiled "glGenTextures"
  GenerateTexture(ptr: any): any {
    let GLctx = this.gameInstance.Module.ctx
    let texture = GLctx.createTexture();

    if (!texture) {
      DCL.GL.recordError(1282);
      this.gameInstance.Module.HEAP32[ptr + 4 >> 2] = 0;
      return
    }

    let id = DCL.GL.getNewId(DCL.GL.textures);
    texture.name = id;
    DCL.GL.textures[id] = texture;
    this.gameInstance.Module.HEAP32[ptr >> 2] = id

    return texture
  }

  StartPlayGIFPromise(frames: any[], texId: any, isWebGL1: boolean): void {
    const promise = PlayGIFPromise(frames, texId, isWebGL1, this.gameInstance.Module.ctx)
    promise.catch((error) => defaultLogger.log(error))
  }
}

async function PlayGIFPromise(frames: any[], texId: any, isWebGL1: boolean, GLctx: any) {
  let frameIndex = 0

  while (true) {
    UpdateGIFTex(frames[frameIndex].imageData, texId, isWebGL1, GLctx)

    // TODO: frames[frameIndex].delay is always 0
    let delay = 320

    await new Promise((resolve) => window.setTimeout(resolve, delay))

    if (++frameIndex === frames.length) {
      frameIndex = 0
    }
  }
}

function UpdateGIFTex(image: any, texId: any, isWebGL1: boolean, GLctx: any) {
  GLctx.bindTexture(GLctx.TEXTURE_2D, DCL.GL.textures[texId])

  if (isWebGL1) {
    GLctx.texImage2D(
      GLctx.TEXTURE_2D,
      0,
      GLctx.RGBA,
      GLctx.RGBA,
      GLctx.UNSIGNED_BYTE,
      image
    )
  } else {
    GLctx.texImage2D(
      GLctx.TEXTURE_2D,
      0,
      GLctx.RGBA,
      image.width,
      image.height,
      0,
      GLctx.RGBA,
      GLctx.UNSIGNED_BYTE,
      image
    )
  }
}
