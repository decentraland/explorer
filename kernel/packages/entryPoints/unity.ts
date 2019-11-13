import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { FAILED_FETCHING_UNITY } from 'shared/loading/types'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import defaultLogger from '../shared/logger'
import { signalRendererInitialized } from '../shared/renderer/actions'
import { lastPlayerPosition, teleportObservable } from '../shared/world/positionThings'
import { HUD, startUnityParcelLoading } from '../unity-interface/dcl'
import { initializeUnity } from '../unity-interface/initializer'
import { experienceStarted } from '../shared/loading/types'
import { OPEN_AVATAR_EDITOR } from '../config/index'

const container = document.getElementById('gameContainer')

declare var global: any

if (!container) throw new Error('cannot find element #gameContainer')

initializeUnity(container)
  .then(async _ => {
    HUD.Minimap.configure({ active: true, visible: true })
    HUD.Avatar.configure({ active: true, visible: true })
    HUD.Notification.configure({ active: true, visible: true })
    HUD.AvatarEditor.configure({ active: true, visible: OPEN_AVATAR_EDITOR })

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
