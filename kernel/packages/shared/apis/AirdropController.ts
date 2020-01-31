import { APIOptions, exposeMethod, registerAPI } from 'decentraland-rpc/lib/host'
import { ExposableAPI } from './ExposableAPI'
import { unityAirdropInterface, AirdropInfo, AirdropInput } from '../airdrops/interface'
import { providerFuture, requestManager } from '../ethereum/provider'
import { getUserAccount } from '../ethereum/EthereumService'
import { Observable } from 'decentraland-ecs/src/ecs/Observable'
import { v4 } from 'uuid'
import { defaultLogger } from '../logger'

export const airdropObservable = new Observable<string>()

@registerAPI('AirdropController')
export class AirdropController extends ExposableAPI {
  private mapIdToTransactions: Record<string, string> = {}
  private mapIdToTargetContract: Record<string, string> = {}

  constructor(options: APIOptions) {
    super(options)

    airdropObservable.add((observedAccept: string) => {
      this.accepted(observedAccept)
    })
  }

  @exposeMethod
  async openCrate(data: AirdropInput, targetContract: string, transaction: string): Promise<void> {
    const unityWindow: unityAirdropInterface = (window as any).unityWindow
    const id = v4()
    this.mapIdToTransactions[id] = transaction
    this.mapIdToTargetContract[id] = targetContract
    unityWindow.TriggerAirdropDisplay({ ...data, id })
  }

  async accepted(id: string) {
    if (this.mapIdToTargetContract[id]) {
      await providerFuture
      const from = await getUserAccount()
      requestManager.eth_sendTransaction({
        from,
        to: this.mapIdToTargetContract[id],
        data: this.mapIdToTransactions[id]
      })
    } else {
      defaultLogger.warn('Received accept of an invalid id')
    }
  }
}
