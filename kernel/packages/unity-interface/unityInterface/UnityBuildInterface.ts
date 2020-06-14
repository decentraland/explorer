/**
 * UnityBuildInterface (previously known as "GameInstance") is the low-level interface exposed
 * by the UnityBuild through which we can send messages to the Unity Build.
 */
export type UnityBuildInterface = {
  SendMessage(object: string, method: string, ...args: (number | string)[]): void
  SetFullscreen(): void
}
