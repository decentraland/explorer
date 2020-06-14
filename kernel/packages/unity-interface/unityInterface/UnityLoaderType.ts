import { UnityBuildInterface } from './UnityBuildInterface'

/**
 * The UnityLoader is the method exposed by the UnityBuild to setup the Unity Framework and create
 * an instance of the UnityBuildInterface.
 *
 * @param divId the `id` of the div in which to place the game, or a reference to the HTMLElement
 * @param manifest the URL of the manifest (the `unity.json` file with information to be loaded)
 */
export type UnityLoaderType = {
  // https://docs.unity3d.com/Manual/webgl-templates.html
  instantiate(divId: string | HTMLElement, manifest: string): UnityBuildInterface
}
