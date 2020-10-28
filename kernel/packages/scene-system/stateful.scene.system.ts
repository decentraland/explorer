import { EventSubscriber, WebWorkerTransport } from 'decentraland-rpc'
import { inject, Script } from 'decentraland-rpc/lib/client/Script'
import { ILogOpts, ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { IEngineAPI } from 'shared/apis/EngineAPI'
import { ParcelIdentity } from 'shared/apis/ParcelIdentity'
import { SceneStateStorageController } from 'shared/apis/SceneStateStorageController'
import { defaultLogger } from 'shared/logger'
import { DevToolsAdapter } from './sdk/DevToolsAdapter'
import { RendererActor } from './stateful-scene/RendererActor'
import { SceneStateDefinition } from './stateful-scene/SceneStateDefinition'

class StatefulWebWorkerScene extends Script {
  @inject('DevTools')
  devTools: any

  @inject('EngineAPI')
  engine!: IEngineAPI

  @inject('ParcelIdentity')
  parcelIdentity!: ParcelIdentity

  @inject('SceneStateStorageController')
  sceneStateStorage!: SceneStateStorageController

  private devToolsAdapter!: DevToolsAdapter
  private renderer!: RendererActor
  private sceneState!: SceneStateDefinition
  private eventSubscriber!: EventSubscriber

  constructor(transport: ScriptingTransport, opt?: ILogOpts) {
    super(transport, opt)
  }

  async systemDidEnable(): Promise<void> {
    this.devToolsAdapter = new DevToolsAdapter(this.devTools)
    const { cid: sceneId } = await this.parcelIdentity.getParcel()
    this.renderer = new RendererActor(this.engine, sceneId)
    this.eventSubscriber = new EventSubscriber(this.engine)

    // Fetch stored scene
    this.sceneState = SceneStateDefinition.fromStoredData(await this.sceneStateStorage.getStoredState(sceneId))

    // Listen to the renderer and update the local scene state
    this.renderer.forwardChangesTo(this.sceneState)

    // Send the initial state ot the renderer
    this.sceneState.sendStateTo(this.renderer)
    this.renderer.sendInitFinished()
    this.log('Sent initial load')

    // Listen to storage requests
    this.eventSubscriber.on('stateEvent', ({ type }) => {
      if (type === 'StoreSceneState') {
        this.sceneStateStorage.storeState(sceneId, this.sceneState.toStorableData())
      }
    })
  }

  // private error(error: Error) {
  //   if (this.devToolsAdapter) {
  //     this.devToolsAdapter.error(error)
  //   } else {
  //     defaultLogger.error('', error)
  //   }
  // }

  private log(...messages: any[]) {
    if (this.devToolsAdapter) {
      this.devToolsAdapter.log(...messages)
    } else {
      defaultLogger.info('', ...messages)
    }
  }
}

// tslint:disable-next-line
new StatefulWebWorkerScene(WebWorkerTransport(self))
