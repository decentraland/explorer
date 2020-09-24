import { ConnectorInterface } from './ConnectorInterface'
import { Magic } from 'magic-sdk'

export class MagicLinkConnector implements ConnectorInterface {
  private magic: Magic

  constructor(config: Map<string, string | boolean>) {
    this.magic = new Magic(config.get('apiKey') as string)
  }

  getProvider() {
    return this.magic.rpcProvider
  }

  async login(values: Map<string, string>) {
    const email = values.get('email')
    if (!email) {
      throw new Error('Email is required')
    }
    const showUI = !values.has('showUI') || !values.get('showUI')
    await this.magic.auth.loginWithMagicLink({ email, showUI })
    return true
  }

  logout(): Promise<boolean> {
    return this.magic.user.logout()
  }
}
