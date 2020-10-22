import { WebWorkerTransport } from 'decentraland-rpc'
import { inject, Script } from 'decentraland-rpc/lib/client/Script'
import { ILogOpts, ScriptingTransport } from 'decentraland-rpc/lib/common/json-rpc/types'
import { IEngineAPI } from 'shared/apis/EngineAPI'
import { ParcelIdentity } from 'shared/apis/ParcelIdentity'
import { defaultLogger } from 'shared/logger'
import { DevToolsAdapter } from './sdk/DevToolsAdapter'
import { UnityActor } from './stateful-scene/UnityActor'

class StatefulWebWorkerScene extends Script {
  @inject('DevTools')
  devTools: any

  @inject('EngineAPI')
  engine!: IEngineAPI

  @inject('ParcelIdentity')
  parcelIdentity!: ParcelIdentity

  private devToolsAdapter!: DevToolsAdapter
  private unity!: UnityActor
  private sceneState: any

  constructor(transport: ScriptingTransport, opt?: ILogOpts) {
    super(transport, opt)
  }

  async systemDidEnable(): Promise<void> {
    this.devToolsAdapter = new DevToolsAdapter(this.devTools)
    const sceneId = this.parcelIdentity.cid
    this.unity = new UnityActor(this.engine, sceneId)

    // Fetch json
    this.sceneState = {
      entities: [
        {
          Transform: {
            position: {
              x: 8,
              y: 0,
              z: 8
            }
          },
          GLTFShape: {
            src: 'models/BlockDog.glb'
          }
        }
      ]
    }

    // Load the initial state
    this.sendInitialState()

    this.log('Sent the initial state')
  }

  private sendInitialState() {
    let ids = 10;
    this.sceneState.entities.forEach((entity: any) => {
      const components = Object.entries(entity).map(([type, data]) => ({ type, data }))
      this.unity.addEntity(`${ids++}`, components)
    })
    this.unity.sendInitFinished()
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
