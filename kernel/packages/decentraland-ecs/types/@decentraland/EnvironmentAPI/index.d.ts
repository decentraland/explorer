declare module '@decentraland/EnvironmentAPI' {
  export type Realm = {
    domain: string
    layer: string
    serverName: string
    displayName: string
  }

  export type ExplorerData = {
    clientUrl: string
    buildNumber: number
    configurations: Record<string, any>
  }

  /**
   * Returns the current connected realm
   */
  export function getCurrentRealm(): Promise<Realm | undefined>

  /**
   * Returns whether the scene is running in preview mode or not
   */
  export function isPreviewMode(): Promise<boolean>


  /**
   * Returns explorer configuration and environment information
   */
  export function getExplorerData(): Promise<ExplorerData>
}
