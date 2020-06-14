import future, { IFuture } from 'fp-future'
import type { Store } from 'redux'
import defaultLogger from './logger'
import { HandlerOfRendererMessages } from '../unity-interface/HandlerOfRendererMessages'
import { rendererInterfaceType } from './renderer-interface/rendererInterface/rendererInterfaceType'
import { builderInterfaceType } from './renderer-interface/builder/builderInterface'
import { browserInterfaceType } from './renderer-interface/browserInterface/browserInterfaceType'
import { RootState } from './store/rootTypes'
import { SceneWorker } from './world/SceneWorker'

declare var globalThis: any

/**
 * Stores all the global state required for both the renderer and the kernel
 */
export const globalDCL: {
  engineInitialized: IFuture<boolean>
  /**
   * Namespace exposed to the Unity Framework
   */
  DCL: {
    EngineStarted: () => void
    MessageFromEngine: HandlerOfRendererMessages
  }
  messageHandler: HandlerOfRendererMessages
  rendererInterface: rendererInterfaceType
  builderInterface: builderInterfaceType
  // @deprecated
  unityInterface: rendererInterfaceType
  globalStore: Store<RootState>
  browserInterface: browserInterfaceType
  analytics: {
    identify: (id: string | any, userData?: { email: string } | any) => void
    user: any
  }
  delighted: any
  /**
   * This variable is used by the Builder and Preview
   */
  currentLoadedScene?: SceneWorker | null
  futures: Record<string, IFuture<any>>
  hasWallet: boolean
  isTheFirstLoading: boolean
} = globalThis

Object.assign(globalDCL, {
  engineInitialized: future<boolean>(),
  DCL: {
    EngineStarted: tooEarlyHandler,
    MessageFromEngine: (type: any, message: any) => {
      defaultLogger.error(`Received message before initialization is ready: ${type}`)
    }
  },
  messageHandler: (type: any, message: any) =>
    defaultLogger.error(`Received message before initialization is ready: ${type}`),
  futures: {},
  isTheFirstLoading: true
})

function tooEarlyHandler() {
  defaultLogger.error(`EngineInitialized called before the Kernel was ready`)
}
