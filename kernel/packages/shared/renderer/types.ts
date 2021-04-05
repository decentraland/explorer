export const RENDERER_INITIALIZED = 'Renderer initialized'
export const PARCEL_LOADING_STARTED = 'Parcel loading started'

export type RendererState = {
  initialized: boolean
  instancedJS: Promise<any> | undefined
}

export type RootRendererState = {
  renderer: RendererState
}

export type UnityLoaderType = {
  // https://docs.unity3d.com/Manual/webgl-templates.html
  instantiate(divId: any, loaderConfig: any): UnityGame
}

export type UnityGame = {
  Module: any
  SendMessage(object: string, method: string, args: number | string): void
  SetFullscreen(): void
}
