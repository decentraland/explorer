import { defaultLogger } from 'shared/logger'
import { ILandToLoadableParcelScene } from 'shared/selectors'
import { ILand, LoadableParcelScene, MappingsResponse, SceneJsonData } from 'shared/types'
import { loadParcelScene, stopParcelSceneWorker } from 'shared/world/parcelSceneManager'
import { UnityParcelScene } from '../UnityParcelScene'
import { globalDCL } from 'shared/globalDCL'

export async function loadPreviewScene() {
  const result = await fetch('/scene.json?nocache=' + Math.random())

  let lastId: string | null = null

  if (globalDCL.currentLoadedScene) {
    lastId = globalDCL.currentLoadedScene.parcelScene.data.sceneId
    stopParcelSceneWorker(globalDCL.currentLoadedScene)
  }

  if (result.ok) {
    // we load the scene to get the metadata
    // about rhe bounds and position of the scene
    // TODO(fmiras): Validate scene according to https://github.com/decentraland/proposals/blob/master/dsp/0020.mediawiki
    const scene = (await result.json()) as SceneJsonData
    const mappingsFetch = await fetch('/mappings')
    const mappingsResponse = (await mappingsFetch.json()) as MappingsResponse

    let defaultScene: ILand = {
      sceneId: 'previewScene',
      baseUrl: location.toString().replace(/\?[^\n]+/g, ''),
      baseUrlBundles: '',
      sceneJsonData: scene,
      mappingsResponse: mappingsResponse
    }

    const parcelScene = new UnityParcelScene(ILandToLoadableParcelScene(defaultScene))
    globalDCL.currentLoadedScene = loadParcelScene(parcelScene)

    const target: LoadableParcelScene = { ...ILandToLoadableParcelScene(defaultScene).data }
    delete target.land

    defaultLogger.info('Reloading scene...')

    if (lastId) {
      globalDCL.rendererInterface.UnloadScene(lastId)
    }

    globalDCL.rendererInterface.LoadParcelScenes([target])

    defaultLogger.info('finish...')

    return defaultScene
  } else {
    throw new Error('Could not load scene.json')
  }
}
