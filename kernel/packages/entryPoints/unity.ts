import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { FAILED_FETCHING_UNITY } from 'shared/loading/types'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import defaultLogger from '../shared/logger'
import { signalRendererInitialized } from '../shared/renderer/actions'
import { lastPlayerPosition, teleportObservable } from '../shared/world/positionThings'
import { startUnityParcelLoading, unityInterface } from '../unity-interface/dcl'
import { initializeUnity } from '../unity-interface/initializer'
import { experienceStarted } from '../shared/loading/types'
import { OPEN_AVATAR_EDITOR, NO_MOTD } from '../config/index'

const container = document.getElementById('gameContainer')

declare var global: any

if (!container) throw new Error('cannot find element #gameContainer')

initializeUnity(container)
  .then(async _ => {
    const i = unityInterface
    
    i.ConfigureMinimapHUD({ active: true, visible: true })
    i.ConfigureAvatarHUD({ active: true, visible: true })
    i.ConfigureNotificationHUD({ active: true, visible: true })
    i.ConfigureAvatarEditorHUD({ active: true, visible: OPEN_AVATAR_EDITOR })
    i.ConfigureSettingsHUD({ active: true, visible: false })
    i.ConfigureExpressionsHUD({ active: true, visible: true })
    i.ConfigurePlayerInfoCardHUD({ active: true, visible: true })

    if (!NO_MOTD)
    {
      //NOTE(Brian): This flow is momentarily deactivated until we find out some way
      //             of getting it externally with the sole exception of buttonAction.
      //
      //             For now, the HUD visuals will be hardcoded in the WelcomeHUD Unity prefab.
      i.ConfigureWelcomeHUD({
        active: true, 
        visible: true, 
        title: "",
        timeTarget: 0,
        timeText: "",
        showTime: true,
        bodyText: "",
        buttonText: "",
        buttonAction: "goto 10,10",
        showButton: true,
      })
    }

    global['globalStore'].dispatch(signalRendererInitialized())
    await startUnityParcelLoading()

    _.instancedJS
      .then($ => {
        teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))
        global['globalStore'].dispatch(experienceStarted())
      })
      .catch(defaultLogger.error)

    document.body.classList.remove('dcl-loading')
    ;(window as any).UnityLoader.Error.handler = (error: any) => {
      console['error'](error)
      ReportFatalError(error.message)
    }
  })
  .catch(err => {
    if (err.message.includes('Authentication error')) {
      // TODO - add some feedback here before reloading - moliva - 22/10/2019
      window.location.reload()
    }

    console['error']('Error loading Unity')
    console['error'](err)
    document.body.classList.remove('dcl-loading')

    ReportFatalError(FAILED_FETCHING_UNITY)
  })
