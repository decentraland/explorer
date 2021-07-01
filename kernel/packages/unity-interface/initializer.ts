// This file decides and loads the renderer of choice

import { initShared } from 'shared'
import { USE_UNITY_INDEXED_DB_CACHE } from 'shared/meta/types'
import { initializeRenderer } from 'shared/renderer/actions'
import { StoreContainer } from 'shared/store/rootTypes'
import { ensureUnityInterface } from 'shared/renderer'
import { CommonRendererOptions, loadUnity } from './loader'
import type { UnityGame } from '@dcl/unity-renderer/index'

import { initializeUnityEditor } from './wsEditorAdapter'
import {
  BringDownClientAndShowError,
  ErrorContext,
  ReportFatalErrorWithUnityPayload
} from 'shared/loading/ReportFatalError'
import { UNEXPECTED_ERROR } from 'shared/loading/types'

declare const globalThis: StoreContainer & { Hls: any }
// HLS is required to make video texture and streaming work in Unity
globalThis.Hls = require('hls.js')

export type InitializeUnityResult = {
  container: HTMLElement
}

async function loadInjectedUnityDelegate(container: HTMLElement, options: CommonRendererOptions): Promise<UnityGame> {
  const queryParams = new URLSearchParams(document.location.search)

  ;(window as any).USE_UNITY_INDEXED_DB_CACHE = USE_UNITY_INDEXED_DB_CACHE

  // inject unity loader
  const { createWebRenderer } = await loadUnity(queryParams.get('renderer') || null, options)

  preventUnityKeyboardLock()

  const canvas = document.createElement('canvas')
  // THIS IS COMMENTED BECAUSE Unity2020 already does this for us
  // 1 canvas.addEventListener('contextmenu', function (e) {
  // 1   e.preventDefault()
  // 1 })
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
async function loadWsEditorDelegate(container: HTMLElement, options: CommonRendererOptions): Promise<UnityGame> {
  const queryParams = new URLSearchParams(document.location.search)

  return initializeUnityEditor(queryParams.get('ws')!, container, options)
}

/** Initialize the injected engine in a container */
export async function initializeUnity(container: HTMLElement): Promise<InitializeUnityResult> {
  const queryParams = new URLSearchParams(document.location.search)

  initShared()

  if (queryParams.has('ws')) {
    // load unity renderer using WebSocket
    globalThis.globalStore.dispatch(initializeRenderer(loadWsEditorDelegate, container))
  } else {
    // load injected renderer
    globalThis.globalStore.dispatch(initializeRenderer(loadInjectedUnityDelegate, container))
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
