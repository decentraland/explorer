import { action } from 'typesafe-actions'

import { RendererInterfaces } from 'unity-interface/dcl'
import { UnityGame } from 'unity-interface/loader'

import { RENDERER_INITIALIZED, PARCEL_LOADING_STARTED } from './types'

export const INITIALIZE_RENDERER = '[Request] Initialize renderer'
export const initializeRenderer = (
  delegate: (container: HTMLElement, onMessage: (type: string, payload: string) => void) => Promise<UnityGame>,
  container: HTMLElement
) => action(INITIALIZE_RENDERER, { delegate, container })
export type InitializeRenderer = ReturnType<typeof initializeRenderer>

export const ENGINE_STARTED = '[Success] Engine started'
export const engineStarted = () => action(ENGINE_STARTED)
export type EngineStarted = ReturnType<typeof engineStarted>

export const RENDERER_ENABLED = '[Succes] Renderer enabled'
export const rendererEnabled = (instancedJS: RendererInterfaces) =>
  action(RENDERER_ENABLED, { instancedJS })
export type RendererEnabled = ReturnType<typeof rendererEnabled>

export const signalRendererInitialized = () => action(RENDERER_INITIALIZED)
export type SignalRendererInitialized = ReturnType<typeof signalRendererInitialized>

export const signalParcelLoadingStarted = () => action(PARCEL_LOADING_STARTED)
export type SignalParcelLoadingStarted = ReturnType<typeof signalParcelLoadingStarted>
