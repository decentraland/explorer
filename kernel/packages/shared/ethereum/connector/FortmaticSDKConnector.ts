import { ConnectorInterface } from './ConnectorInterface'
import { PhantomMode } from 'fortmatic/dist/cjs/src/core/sdk'
import { PhantomUser } from 'fortmatic/dist/cjs/src/modules/phantom-mode/phantom-user'

const Fortmatic = require('fortmatic')

export class FortmaticSDKConnector implements ConnectorInterface {
  private user: PhantomUser | undefined
  private readonly phantom: PhantomMode

  constructor(config: Map<string, string | boolean>) {
    this.phantom = new Fortmatic.Phantom(config.get('apiKey') as string, config.get('network') as string)
  }

  getProvider(): any {
    return this.phantom
  }

  async login(values: Map<string, string | boolean>) {
    const email = values.get('email') as string
    const showUI = !values.has('showUI') || !values.get('showUI')
    this.user = await this.phantom.loginWithMagicLink({ email, showUI })
    return this.phantom
  }

  async logout() {
    if (!this.user) {
      return true
    }
    return this.user.logout()
  }
}
