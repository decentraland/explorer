import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { unityInterface } from 'unity-interface/dcl'

export interface IUserActionModule {
  requestTeleport(destination: string): Promise<void>
}

@registerAPI('UserActionModule')
export class UserActionModule extends ExposableAPI implements IUserActionModule {
  @exposeMethod
  async requestTeleport(destination: string): Promise<void> {
    unityInterface.RequestTeleport(destination)
  }
}
