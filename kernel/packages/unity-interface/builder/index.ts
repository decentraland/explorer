import { globalDCL } from 'shared/globalDCL'
import { ILandToLoadableParcelScene, ILandToLoadableParcelSceneUpdate } from 'shared/selectors'
import { ILand, LoadableParcelScene } from 'shared/types'
import { loadParcelScene, stopParcelSceneWorker } from 'shared/world/parcelSceneManager'
import { UnityParcelScene } from '../UnityParcelScene'

export function loadBuilderScene(sceneData: ILand) {
  unloadCurrentBuilderScene()

  const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(sceneData))
  globalDCL.currentLoadedScene = loadParcelScene(parcelScene)

  const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(sceneData).data }
  delete target.land

  globalDCL.rendererInterface.LoadParcelScenes([target])
  return parcelScene
}

export function unloadCurrentBuilderScene() {
  if (globalDCL.currentLoadedScene) {
    const parcelScene = globalDCL.currentLoadedScene.parcelScene as UnityParcelScene
    parcelScene.emit('builderSceneUnloaded', {})

    stopParcelSceneWorker(globalDCL.currentLoadedScene)
    globalDCL.rendererInterface.SendBuilderMessage('UnloadBuilderScene', parcelScene.data.sceneId)
    globalDCL.currentLoadedScene = null
  }
}

export function updateBuilderScene(sceneData: ILand) {
  if (globalDCL.currentLoadedScene) {
    const target: LoadableParcelScene = { ...ILandToLoadableParcelSceneUpdate(sceneData).data }
    delete target.land
    globalDCL.rendererInterface.UpdateParcelScenes([target])
  }
}
