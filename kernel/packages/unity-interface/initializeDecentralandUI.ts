import { globalDCL } from 'shared/globalDCL'
import { getParcelSceneID, loadParcelScene } from 'shared/world/parcelSceneManager'
import { hudWorkerUrl } from 'shared/world/SceneWorker'
import { ensureUiApis } from 'shared/world/uiSceneInitializer'
import { UnityScene } from './UnityScene'

export async function initializeDecentralandUI() {
  const sceneId = 'dcl-ui-scene'

  const scene = new UnityScene({
    sceneId,
    name: 'ui',
    baseUrl: location.origin,
    main: hudWorkerUrl,
    useFPSThrottling: false,
    data: {},
    mappings: []
  })

  const worker = loadParcelScene(scene)
  worker.persistent = true

  await ensureUiApis(worker)

  globalDCL.rendererInterface.CreateUIScene({ id: getParcelSceneID(scene), baseUrl: scene.data.baseUrl })
}
