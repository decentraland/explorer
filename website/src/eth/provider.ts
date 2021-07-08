import { connection, ConnectionResponse, ProviderType } from "decentraland-connect"
import { WebSocketProvider } from "eth-connect"
import { ChainId } from "@dcl/schemas"

export const chainIdRpc = new Map<number, string>([
  [1, "wss://mainnet.infura.io/ws/v3/074a68d50a7c4e6cb46aec204a50cbf0"],
  [3, "wss://ropsten.infura.io/ws/v3/074a68d50a7c4e6cb46aec204a50cbf0"],
])

export async function getEthereumProvider(type: ProviderType | null, chainId: ChainId): Promise<{ sendAsync: any }> {
  if (type === null) {
    const rpc = chainIdRpc.get(chainId)
    if (!rpc) throw new Error("Can't get RPC for chainId " + chainId)
    return new WebSocketProvider(rpc)
  }

  const result = await connection.connect(type, chainId)

  return result.provider
}

export async function restoreConnection(): Promise<{ sendAsync: any } | null> {
  try {
    const result = await connection.tryPreviousConnection()
    return result.provider
  } catch (err) {
    return null
  }
}
