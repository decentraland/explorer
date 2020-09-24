import { ConnectorInterface } from './ConnectorInterface'

const Fortmatic = require('fortmatic')

export class FortmaticConnector implements ConnectorInterface {
  private fm: any

  constructor(options: Map<string, string | boolean>) {
    this.fm = new Fortmatic(options.get('apiKey'))
  }

  getProvider(): any {
    return this.fm.getProvider()
  }

  async login(values: Map<string, string>) {
    return this.fm.user.login()
  }

  async logout() {
    return this.fm.user.logout()
  }
}
