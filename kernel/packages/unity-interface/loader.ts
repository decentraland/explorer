import future from 'fp-future'
import { parseUrn } from '@dcl/urn-resolver'
import type * as _TheRenderer from '@dcl/unity-renderer/index'
import { trackEvent } from 'shared/analytics'

declare const globalThis: any

const rendererPackageJson = require('@dcl/unity-renderer/package.json')

export type DclRenderer = typeof _TheRenderer

export type LoadRendererResult = {
  DclRenderer: DclRenderer
  createUnityInstance: (canvas: HTMLCanvasElement, onProgress?: (progress: number) => void) => Promise<UnityGame>
  baseUrl: string
}

export type UnityGame = {
  Module: {
    /** this handler can be overwritten, return true to stop error propagation */
    errorHandler?: (message: string, filename: string, lineno: number) => boolean
  }
  SendMessage(object: string, method: string, args: number | string): void
  SetFullscreen(): void
  Quit(): Promise<void>
}

async function injectRenderer(baseUrl: string, version: string): Promise<LoadRendererResult> {
  const scriptUrl = new URL('index.js?v=' + version, baseUrl).toString()
  window['console'].log('Renderer: ' + scriptUrl)

  const startLoadingTime = performance.now()

  trackEvent('unity_loader_downloading_start', { version, loading_time: performance.now() - startLoadingTime })
  await injectScript(scriptUrl)
  trackEvent('unity_loader_downloading_end', { version, loading_time: performance.now() - startLoadingTime })

  if (typeof globalThis.createUnityInstance === 'undefined') {
    throw new Error('Error while loading createUnityInstance from ' + scriptUrl)
  }

  if (typeof globalThis.DclRenderer === 'undefined') {
    throw new Error('Error while loading the renderer from ' + scriptUrl)
  }

  const originalCreateUnityInstance: (
    canvas: HTMLCanvasElement,
    config: any,
    onProgress?: (progress: number) => void
  ) => Promise<UnityGame> = globalThis.createUnityInstance

  return {
    DclRenderer: globalThis.DclRenderer,
    createUnityInstance: async (canvas, onProgress?) => {
      const resolveWithBaseUrl = (file: string) => new URL(file + '?v=' + version, baseUrl).toString()
      const config = {
        dataUrl: resolveWithBaseUrl('unity.data.unityweb'),
        frameworkUrl: resolveWithBaseUrl('unity.framework.js.unityweb'),
        codeUrl: resolveWithBaseUrl('unity.wasm.unityweb'),
        streamingAssetsUrl: 'StreamingAssets',
        companyName: 'Decentraland',
        productName: 'Decentraland World Client',
        productVersion: '0.1'
      }

      let didLoadUnity = false

      trackEvent('unity_downloading_start', { version, loading_time: performance.now() - startLoadingTime })

      return originalCreateUnityInstance(canvas, config, function (...args) {
        // 0.9 is harcoded in unityLoader, it marks the download-complete event

        if (0.9 == args[0] && !didLoadUnity) {
          trackEvent('unity_downloading_end', { version, loading_time: performance.now() - startLoadingTime })
          trackEvent('unity_initializing_start', { version, loading_time: performance.now() - startLoadingTime })
          didLoadUnity = true
        }
        // 1.0 marks the engine-initialized event
        if (1.0 == args[0]) {
          trackEvent('unity_initializing_end', { version, loading_time: performance.now() - startLoadingTime })
        }
        if (onProgress) return onProgress.apply(null, args)
      })
    },
    baseUrl
  }
}

async function loadDefaultRenderer(): Promise<LoadRendererResult> {
  // PAY ATTENTION:
  //  Whenever we decide to not bundle the renderer anymore and have independant
  //  release cycles for the explorer, replace this whole function by the following commented line
  //
  // > return loadRendererByBranch('master')

  function getRendererArtifactsRoot() {
    // This function is used by preview, instead of using "." as root,
    // preview uses '/@/artifacts'
    if (typeof globalThis.RENDERER_ARTIFACTS_ROOT === 'undefined') {
      throw new Error('RENDERER_ARTIFACTS_ROOT is undefined')
    } else {
      return new URL(globalThis.RENDERER_ARTIFACTS_ROOT, document.location.toString()).toString()
    }
  }

  // Load the embeded renderer from the artifacts root folder
  return injectRenderer(getRendererArtifactsRoot(), rendererPackageJson.version)
}

async function loadRendererByBranch(branch: string): Promise<LoadRendererResult> {
  const baseUrl = `https://renderer-artifacts.decentraland.org/branch/${branch}/`
  return injectRenderer(baseUrl, performance.now().toString())
}

export async function loadUnity(urn?: string): Promise<LoadRendererResult> {
  if (!urn) {
    return loadDefaultRenderer()
  } else {
    const parsedUrn = await parseUrn(urn)

    if (!parsedUrn) {
      throw new Error('An invalid urn was provided for the renderer')
    }

    // urn:decentraland:off-chain:renderer-artifacts:${branch}
    if (parsedUrn.type === 'off-chain' && parsedUrn.registry === 'renderer-artifacts') {
      return loadRendererByBranch(parsedUrn.id)
    }

    throw new Error('It was impossible to resolve a renderer for the URN "' + urn + '"')
  }
}

async function injectScript(url: string) {
  const theFuture = future<Event>()
  const theScript = document.createElement('script')
  theScript.src = url
  theScript.async = true
  theScript.type = 'application/javascript'
  theScript.addEventListener('load', theFuture.resolve)
  theScript.addEventListener('error', (e) => theFuture.reject(e.error || (e as any)))
  document.body.appendChild(theScript)
  return theFuture
}
