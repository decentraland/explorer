import future from 'fp-future'
import { parseUrn } from '@dcl/urn-resolver'
import type * as _TheRenderer from '@dcl/unity-renderer/src/index'
import { trackEvent } from 'shared/analytics'

declare const globalThis: { DclRenderer?: DclRenderer }

const rendererPackageJson = require('@dcl/unity-renderer/package.json')

export type DclRenderer = typeof _TheRenderer

export type LoadRendererResult = {
  DclRenderer: DclRenderer
  baseUrl: string
  createWebRenderer(canvas: HTMLCanvasElement): Promise<_TheRenderer.DecentralandRendererInstance>
}

/**
 * The following options are common to all kinds of renderers, it abstracts
 * what we need to implement in our end to support a renderer. WIP
 */
export type CommonRendererOptions = {
  onMessage: (type: string, payload: string) => void
}

async function injectRenderer(
  baseUrl: string,
  rendererVersion: string,
  options: CommonRendererOptions
): Promise<LoadRendererResult> {
  const scriptUrl = new URL('index.js?v=' + rendererVersion, baseUrl).toString()
  window['console'].log('Renderer: ' + scriptUrl)

  let startTime = performance.now()

  trackEvent('unity_loader_downloading_start', { renderer_version: rendererVersion })
  await injectScript(scriptUrl)
  trackEvent('unity_loader_downloading_end', {
    renderer_version: rendererVersion,
    loading_time: performance.now() - startTime
  })

  if (typeof globalThis.DclRenderer === 'undefined') {
    throw new Error('Error while loading the renderer from ' + scriptUrl)
  }

  if (typeof (globalThis.DclRenderer.initializeWebRenderer as any) === 'undefined') {
    throw new Error(
      'This version of explorer is only compatible with renderers newer than https://github.com/decentraland/unity-renderer/pull/689'
    )
  }

  return {
    DclRenderer: globalThis.DclRenderer,
    createWebRenderer: async (canvas) => {
      let didLoadUnity = false

      startTime = performance.now()
      trackEvent('unity_downloading_start', { renderer_version: rendererVersion })

      function onProgress(progress: number) {
        // 0.9 is harcoded in unityLoader, it marks the download-complete event
        if (0.9 === progress && !didLoadUnity) {
          trackEvent('unity_downloading_end', {
            renderer_version: rendererVersion,
            loading_time: performance.now() - startTime
          })

          startTime = performance.now()
          trackEvent('unity_initializing_start', { renderer_version: rendererVersion })
          didLoadUnity = true
        }
        // 1.0 marks the engine-initialized event
        if (1.0 === progress) {
          trackEvent('unity_initializing_end', {
            renderer_version: rendererVersion,
            loading_time: performance.now() - startTime
          })
        }
      }

      return globalThis.DclRenderer!.initializeWebRenderer({
        baseUrl,
        canvas,
        versionQueryParam: rendererVersion,
        onProgress,
        onMessageLegacy: options.onMessage
      })
    },
    baseUrl
  }
}

async function loadDefaultRenderer(
  rootArtifactsUrl: string,
  options: CommonRendererOptions
): Promise<LoadRendererResult> {
  // PAY ATTENTION:
  //  Whenever we decide to not bundle the renderer anymore and have independant
  //  release cycles for the explorer, replace this whole function by the following commented line
  //
  // > return loadRendererByBranch('master')

  // Load the embeded renderer from the artifacts root folder
  return injectRenderer(rootArtifactsUrl, rendererPackageJson.version, options)
}

async function loadRendererByBranch(branch: string, options: CommonRendererOptions): Promise<LoadRendererResult> {
  const baseUrl = `https://renderer-artifacts.decentraland.org/branch/${branch}/`
  return injectRenderer(baseUrl, performance.now().toString(), options)
}

export async function loadUnity(
  urn: string | null,
  rootArtifactsUrl: string,
  options: CommonRendererOptions
): Promise<LoadRendererResult> {
  if (urn === null) {
    return loadDefaultRenderer(rootArtifactsUrl, options)
  } else {
    const parsedUrn = await parseUrn(urn)

    if (!parsedUrn) {
      throw new Error('An invalid urn was provided for the renderer')
    }

    // urn:decentraland:off-chain:renderer-artifacts:${branch}
    if (parsedUrn.type === 'off-chain' && parsedUrn.registry === 'renderer-artifacts') {
      return loadRendererByBranch(parsedUrn.id, options)
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
