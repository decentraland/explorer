import { action } from 'typesafe-actions'

import type { CommonRendererOptions } from 'unity-interface/loader'
import type { UnityGame } from '@dcl/unity-renderer/src/index'

import { RENDERER_INITIALIZED, PARCEL_LOADING_STARTED } from './types'

export const INITIALIZE_RENDERER = '[Request] Initialize renderer'
export const initializeRenderer = (
  delegate: (container: HTMLElement, options: CommonRendererOptions) => Promise<UnityGame>,
  container: HTMLElement
) => action(INITIALIZE_RENDERER, { delegate, container })
export type InitializeRenderer = ReturnType<typeof initializeRenderer>

export const signalRendererInitialized = () => action(RENDERER_INITIALIZED)
export type SignalRendererInitialized = ReturnType<typeof signalRendererInitialized>

export const signalParcelLoadingStarted = () => action(PARCEL_LOADING_STARTED)
export type SignalParcelLoadingStarted = ReturnType<typeof signalParcelLoadingStarted>
