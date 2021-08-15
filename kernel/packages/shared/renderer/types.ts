export const RENDERER_INITIALIZED = 'Renderer initialized'
export const PARCEL_LOADING_STARTED = 'Parcel loading started'

export type RendererState = {
  initialized: boolean
  parcelLoadingStarted: boolean
}

export type RootRendererState = {
  renderer: RendererState
}
