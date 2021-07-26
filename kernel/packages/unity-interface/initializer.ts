// This file decides and loads the renderer of choice

import { USE_UNITY_INDEXED_DB_CACHE } from 'shared/meta/types'
import { initializeRenderer } from 'shared/renderer/actions'
import { ensureUnityInterface } from 'shared/renderer'
import { CommonRendererOptions, loadUnity } from './loader'
import type { UnityGame } from '@dcl/unity-renderer/src/index'
import type { KernelOptions } from '@dcl/kernel-interface'

import { initializeUnityEditor } from './wsEditorAdapter'
import {
  BringDownClientAndShowError,
  ErrorContext,
  ReportFatalErrorWithUnityPayload
} from 'shared/loading/ReportFatalError'
import { UNEXPECTED_ERROR } from 'shared/loading/types'
import { store } from 'shared/store/isolatedStore'
import defaultLogger from 'shared/logger'
import { browserInterface } from './BrowserInterface'

declare const globalThis: { Hls: any }
// HLS is required to make video texture and streaming work in Unity
globalThis.Hls = require('hls.js')

export type InitializeUnityResult = {
  container: HTMLElement
}

const rendererOptions: Partial<KernelOptions['rendererOptions']> = {}

const defaultOptions: CommonRendererOptions = {
  onMessage(type: string, jsonEncodedMessage: string) {
    let parsedJson = null
    try {
      parsedJson = JSON.parse(jsonEncodedMessage)
    } catch (e) {
      // we log the whole message to gain visibility
      defaultLogger.error(e.message + ' messageFromEngine: ' + type + ' ' + jsonEncodedMessage)
      throw e
    }
    // this is outside of the try-catch to enable V8 path optimizations
    // keep the following line outside the `try`
    browserInterface.handleUnityMessage(type, parsedJson)
  }
}

async function loadInjectedUnityDelegate(container: HTMLElement): Promise<UnityGame> {
  ;(window as any).USE_UNITY_INDEXED_DB_CACHE = USE_UNITY_INDEXED_DB_CACHE

  // inject unity loader
  const rootArtifactsUrl = rendererOptions.baseUrl || ''
  const { createWebRenderer } = await loadUnity(rootArtifactsUrl, defaultOptions)

  preventUnityKeyboardLock()

  const canvas = document.createElement('canvas')
  canvas.id = '#canvas'
  container.appendChild(canvas)

  const { originalUnity, engineStartedFuture } = await createWebRenderer(canvas)

  // TODO: move to unity-renderer js project
  originalUnity.Module.errorHandler = (message: string, filename: string, lineno: number) => {
    console['error'](message, filename, lineno)

    if (message.includes('The error you provided does not contain a stack trace')) {
      // This error is something that react causes only on development, with unhandled promises and strange errors with no stack trace (i.e, matrix errors).
      // Some libraries (i.e, matrix client) don't handle promises well and we shouldn't crash the explorer because of that
      return true
    }

    const error = new Error(`${message} ... file: ${filename} - lineno: ${lineno}`)
    ReportFatalErrorWithUnityPayload(error, ErrorContext.RENDERER_ERRORHANDLER)
    BringDownClientAndShowError(UNEXPECTED_ERROR)
    return true
  }

  await engineStartedFuture

  return originalUnity
}

/** Initialize engine using WS transport (UnityEditor) */
async function loadWsEditorDelegate(container: HTMLElement): Promise<UnityGame> {
  const queryParams = new URLSearchParams(document.location.search)

  return initializeUnityEditor(queryParams.get('ws')!, container, defaultOptions)
}

/** Initialize the injected engine in a container */
export async function initializeUnity(options: KernelOptions['rendererOptions']): Promise<InitializeUnityResult> {
  const queryParams = new URLSearchParams(document.location.search)

  Object.assign(rendererOptions, options)
  const { container } = rendererOptions

  if (queryParams.has('ws')) {
    // load unity renderer using WebSocket
    store.dispatch(initializeRenderer(loadWsEditorDelegate, container))
  } else {
    // load injected renderer
    store.dispatch(initializeRenderer(loadInjectedUnityDelegate, container))
  }

  await ensureUnityInterface()

  return {
    container
  }
}

/**
 * Prevent unity from locking the keyboard when there is an
 * active element (like delighted textarea)
 */
function preventUnityKeyboardLock() {
  const originalFunction = window.addEventListener
  window.addEventListener = function (event: any, handler: any, options?: any) {
    if (['keypress', 'keydown', 'keyup'].includes(event)) {
      originalFunction.call(
        window,
        event,
        (e) => {
          if (!document.activeElement || document.activeElement === document.body) {
            handler(e)
          }
        },
        options
      )
    } else {
      originalFunction.call(window, event, handler, options)
    }
    return true
  }
}
