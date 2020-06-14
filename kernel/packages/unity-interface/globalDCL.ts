import future, { IFuture } from 'fp-future'
import type { Store } from 'redux'
import defaultLogger from 'shared/logger'
import { HandlerOfRendererMessages } from './HandlerOfRendererMessages'
import { unityInterfaceType } from './unityInterface/unityInterfaceType'
import { browserInterfaceType } from './browserInterface/browserInterfaceType'
import { UnityBuildInterface } from './unityInterface/UnityBuildInterface'
import { RootState } from 'shared/store/rootTypes'
import { SceneWorker } from 'shared/world/SceneWorker'

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
  unityInterface: unityInterfaceType
  gameInstance: UnityBuildInterface
  globalStore: Store<RootState>
  browserInterface: browserInterfaceType
  analytics: {
    identify: (id: string, userData: { email: string }) => void
  }
  /**
   * This variable is used by the Builder and Preview
   */
  currentLoadedScene?: SceneWorker | null
  futures: Record<string, IFuture<any>>
} = globalThis

Object.assign(globalDCL, {
  engineInitialized: future<boolean>(),
  DCL: {
    EngineStarted: tooEarlyHandler,
    MessageFromEngine: (type: any, message: any) => {
      defaultLogger.error(`Received message before initialization is ready: ${type}`)
    },
  },
  messageHandler: (type: any, message: any) =>
    defaultLogger.error(`Received message before initialization is ready: ${type}`),
  futures: {},
})

function tooEarlyHandler() {
  defaultLogger.error(`EngineInitialized called before the Kernel was ready`)
}
