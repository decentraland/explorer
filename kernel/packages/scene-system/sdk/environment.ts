import { DecentralandInterface } from 'decentraland-ecs/src'

export function getDclEnvironment(dcl: DecentralandInterface) {
  // implement fetch as defined in kernel/packages/decentraland-ecs/types/env/index.d.ts
  const sceneFetch: typeof fetch = function (a: any, b?: any) {
    return fetch.call(null, a, b)
  }

  const internalSocket = Symbol('internalSocket')

  // implements websocket as defined in kernel/packages/decentraland-ecs/types/env/index.d.ts
  class SceneWebSocket /* implements ecs.WebSocket */ {
    static CLOSED = WebSocket.CLOSED
    static CLOSING = WebSocket.CLOSING
    static CONNECTING = WebSocket.CONNECTING
    static OPEN = WebSocket.OPEN
    readonly CLOSED: number = SceneWebSocket.CLOSED
    readonly CLOSING: number = SceneWebSocket.CLOSING
    readonly CONNECTING: number = SceneWebSocket.CONNECTING
    readonly OPEN: number = SceneWebSocket.OPEN

    get onclose() {
      return this[internalSocket].onclose
    }
    get onerror() {
      return this[internalSocket].onerror
    }
    get onmessage() {
      return this[internalSocket].onmessage
    }
    get onopen() {
      return this[internalSocket].onopen
    }
    set onclose(v: WebSocket['onclose']) {
      this[internalSocket].onclose = v
    }
    set onerror(v: WebSocket['onerror']) {
      this[internalSocket].onerror = v
    }
    set onmessage(v: WebSocket['onmessage']) {
      this[internalSocket].onmessage = v
    }
    set onopen(v: WebSocket['onopen']) {
      this[internalSocket].onopen = v
    }

    get bufferedAmount(): number {
      return this[internalSocket].bufferedAmount
    }
    get extensions(): string {
      return this[internalSocket].extensions
    }
    get readyState(): number {
      return this[internalSocket].readyState
    }
    get protocol(): string {
      return this[internalSocket].protocol
    }

    private [internalSocket]: WebSocket

    constructor(public readonly url: string, public readonly protocols?: string | string[]) {
      this[internalSocket] = new WebSocket(url, protocols)
    }

    close(code?: number, reason?: string): void {
      this[internalSocket].close(code, reason)
    }
    send(data: string): void {
      this[internalSocket].send(data)
    }
  }

  return {
    dcl,
    fetch: sceneFetch,
    WebSocket: SceneWebSocket,
    Map,
    Set,
    WeakMap,
    WeakSet,
    window: undefined
  }
}
