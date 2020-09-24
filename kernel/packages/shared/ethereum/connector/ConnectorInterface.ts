export interface ConnectorInterface {
  getProvider(): any

  login(values: Map<string, string>): Promise<any>

  logout(): Promise<boolean>
}
