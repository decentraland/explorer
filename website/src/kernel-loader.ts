import future from 'fp-future'
import { KernelOptions, KernelResult, IDecentralandKernel } from '../../anti-corruption-layer/kernel-types'

export async function injectKernel(options: KernelOptions): Promise<KernelResult> {
  const kernelUrl = new URL(`website.js`, options.kernelOptions.baseUrl).toString()

  Object.assign(globalThis, {
    RENDERER_ARTIFACTS_ROOT: options.rendererOptions.baseUrl
  })

  await injectScript(kernelUrl)

  const DecentralandKernel: IDecentralandKernel = (globalThis as any).DecentralandKernel

  if (!DecentralandKernel) throw new Error('DecentralandKernel could not be loaded')

  return await DecentralandKernel.initKernel(options)
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
