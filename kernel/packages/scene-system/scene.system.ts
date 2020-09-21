import { WebWorkerTransport } from 'decentraland-rpc'
import { inject } from 'decentraland-rpc/lib/client/Script'
import { defaultLogger } from 'shared/logger'
import { DclShell } from './sdk/DclShell'
import { DevToolsAdapter } from './sdk/DevToolsAdapter'
import { customEval, getES5Context } from './sdk/sandbox'

/**
 * This file starts the scene in a WebWorker context.
 */

class WebWorkerScene extends DclShell {
  @inject('DevTools')
  devTools: any

  devToolsAdapter?: DevToolsAdapter

  async runCode(source: string, env: any): Promise<void> {
    await customEval(source, getES5Context(env))
  }

  async systemDidEnable() {
    this.devToolsAdapter = new DevToolsAdapter(this.devTools)
    await super.systemDidEnable()
  }

  onError(error: Error) {
    if (this.devToolsAdapter) {
      this.devToolsAdapter.error(error)
    } else {
      defaultLogger.error('', error)
    }
  }

  onLog(...messages: any[]) {
    if (this.devToolsAdapter) {
      this.devToolsAdapter.log(...messages)
    } else {
      defaultLogger.info('', ...messages)
    }
  }

  startLoop() {
    let start = performance.now()

    const update = () => {
      const now = performance.now()
      const dt = now - start
      start = now

      setTimeout(update, this.updateInterval)

      let time = dt / 1000

      this.update(time)
    }

    update()
  }
}

// tslint:disable-next-line
new WebWorkerScene(WebWorkerTransport(self))
