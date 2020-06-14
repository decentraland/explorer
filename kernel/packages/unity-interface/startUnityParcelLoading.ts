import { providerFuture } from 'shared/ethereum/provider'
import { globalDCL } from 'shared/globalDCL'
import { aborted } from 'shared/loading/ReportFatalError'
import { loadingScenes } from 'shared/loading/types'
import { ILandToLoadableParcelScene } from 'shared/selectors'
import { enableParcelSceneLoading } from 'shared/world/parcelSceneManager'
import { UnityParcelScene } from './dcl'

export async function startUnityParcelLoading() {
  const p = await providerFuture
  globalDCL.hasWallet = p.successful

  globalDCL.globalStore.dispatch(loadingScenes())
  await enableParcelSceneLoading({
    parcelSceneClass: UnityParcelScene,
    preloadScene: async _land => {
      // TODO:
      // 1) implement preload call
      // 2) await for preload message or timeout
      // 3) return
    },
    onLoadParcelScenes: lands => {
      globalDCL.rendererInterface.LoadParcelScenes(
        lands.map($ => {
          const x = Object.assign({}, ILandToLoadableParcelScene($).data)
          delete x.land
          return x
        })
      )
    },
    onUnloadParcelScenes: lands => {
      lands.forEach($ => {
        globalDCL.rendererInterface.UnloadScene($.sceneId)
      })
    },
    onPositionSettled: spawnPoint => {
      if (!aborted) {
        globalDCL.rendererInterface.Teleport(spawnPoint)
        globalDCL.rendererInterface.ActivateRendering()
      }
    },
    onPositionUnsettled: () => {
      globalDCL.rendererInterface.DeactivateRendering()
    }
  })
}
