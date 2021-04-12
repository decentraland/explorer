import { EventSubscriber, WebWorkerTransport } from 'decentraland-rpc'
import { inject, Script } from 'decentraland-rpc/lib/client/Script'
import { ILogOpts, ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { IEngineAPI } from 'shared/apis/EngineAPI'
import { ParcelIdentity } from 'shared/apis/ParcelIdentity'
import { SceneStateStorageController } from 'shared/apis/SceneStateStorageController/SceneStateStorageController'
import { defaultLogger } from 'shared/logger'
import { DevToolsAdapter } from './sdk/DevToolsAdapter'
import { RendererStatefulActor } from './stateful-scene/RendererStatefulActor'
import { BuilderStatefulActor } from './stateful-scene/BuilderStatefulActor'
import { SceneStateActor as SceneStatefulActor } from './stateful-scene/SceneStateActor'
import { serializeSceneState } from './stateful-scene/SceneStateDefinitionSerializer'
import { EnvironmentAPI } from 'shared/apis/EnvironmentAPI'

class StatefulWebWorkerScene extends Script {
  @inject('DevTools')
  devTools: any

  @inject('EngineAPI')
  engine!: IEngineAPI

  @inject('EnvironmentAPI')
  environmentAPI!: EnvironmentAPI

  @inject('ParcelIdentity')
  parcelIdentity!: ParcelIdentity

  @inject('SceneStateStorageController')
  sceneStateStorage!: SceneStateStorageController

  private devToolsAdapter!: DevToolsAdapter
  private rendererActor!: RendererStatefulActor
  private builderActor!: BuilderStatefulActor
  private sceneActor!: SceneStatefulActor
  private eventSubscriber!: EventSubscriber

  constructor(transport: ScriptingTransport, opt?: ILogOpts) {
    super(transport, opt)
  }

  async systemDidEnable(): Promise<void> {
    const currentRealm = await this.environmentAPI.getCurrentRealm()
    this.devToolsAdapter = new DevToolsAdapter(this.devTools)
    const { cid: sceneId, land: land } = await this.parcelIdentity.getParcel()
    this.rendererActor = new RendererStatefulActor(this.engine, sceneId)
    this.eventSubscriber = new EventSubscriber(this.engine)
    this.builderActor = new BuilderStatefulActor(land, currentRealm!.domain, this.sceneStateStorage)

    // Fetch stored scene
    const initialState = await this.builderActor.getInititalSceneState()

    this.sceneActor = new SceneStatefulActor(this.engine, initialState)

    // Listen to the renderer and update the local scene state
    this.rendererActor.forwardChangesTo(this.sceneActor)

    // Send the initial state ot the renderer
    this.sceneActor.sendStateTo(this.rendererActor)

    this.rendererActor.sendInitFinished()
    this.log('Sent initial load')

    // Listen to scene state events
    this.ListenToEvents(sceneId)
  }

  private ListenToEvents(sceneId: string): void {
    // Listen to storage requests
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      if (data.type === 'StoreSceneState') {
        this.sceneStateStorage
          .storeState(sceneId, serializeSceneState(this.sceneActor.getState()))
          .catch((error) => this.error(`Failed to store the scene's state`, error))
      }
    })

    // Listen to save scene requests
    this.eventSubscriber.on('stateEvent', ({ data }) => {
      if (data.type === 'SaveSceneState') {
        this.sceneStateStorage
          .saveProjectManifest(serializeSceneState(this.sceneActor.getState()))
          .catch((error) => this.error(`Failed to save the scene's manifest`, error))
      }
    })
  }

  private error(context: string, error: Error) {
    if (this.devToolsAdapter) {
      this.devToolsAdapter.error(error)
    } else {
      defaultLogger.error(context, error)
    }
  }

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
