import { Observable } from '../../decentraland-ecs/src/ecs/Observable'
import { Stats } from './debug'

export type BrokerMessage = {
  data: Uint8Array
  channel: string
}

export interface IBrokerConnection {
  stats: Stats | null
  onMessageObservable: Observable<BrokerMessage>
  readonly hasUnreliableChannel: boolean
  readonly hasReliableChannel: boolean
  readonly isAuthenticated: boolean
  readonly isConnected: Promise<void>
  sendReliable(data: Uint8Array): void
  sendUnreliable(data: Uint8Array): void
  printDebugInformation(): void
  close(): void
}
