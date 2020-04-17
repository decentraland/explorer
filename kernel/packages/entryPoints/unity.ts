declare const globalThis: { UnityLoader: any } & StoreContainer
declare const global: any

// IMPORTANT! This should be execd before loading 'config' module to ensure that init values are successfully loaded
global.enableWeb3 = true

import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { experienceStarted, NOT_INVITED, AUTH_ERROR_LOGGED_OUT, FAILED_FETCHING_UNITY } from 'shared/loading/types'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import { NO_MOTD, tutorialEnabled } from '../config/index'
import defaultLogger from 'shared/logger'
import { signalRendererInitialized, signalParcelLoadingStarted } from 'shared/renderer/actions'
import { lastPlayerPosition, teleportObservable } from 'shared/world/positionThings'
import { StoreContainer } from 'shared/store/rootTypes'
import { hasWallet, startUnityParcelLoading, unityInterface } from '../unity-interface/dcl'
import { initializeUnity } from '../unity-interface/initializer'
import { HUDElementID } from 'shared/types'
const container = document.getElementById('gameContainer')

if (!container) throw new Error('cannot find element #gameContainer')

initializeUnity(container)
  .then(async _ => {
    const i = unityInterface
    i.ConfigureHUDElement(HUDElementID.MINIMAP, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.AVATAR, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.NOTIFICATION, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.AVATAR_EDITOR, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.SETTINGS, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.EXPRESSIONS, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.PLAYER_INFO_CARD, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.AIRDROPPING, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.TERMS_OF_SERVICE, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.TASKBAR, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.WORLD_CHAT_WINDOW, { active: true, visible: true })

    globalThis.globalStore.dispatch(signalRendererInitialized())

    await startUnityParcelLoading()

    globalThis.globalStore.dispatch(signalParcelLoadingStarted())

    if (!NO_MOTD) {
      i.ConfigureHUDElement(HUDElementID.WELCOME, { active: false, visible: !tutorialEnabled() })
    }

    _.instancedJS
      .then(() => {
        teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))
        globalThis.globalStore.dispatch(experienceStarted())
      })
      .catch(defaultLogger.error)

    document.body.classList.remove('dcl-loading')
    globalThis.UnityLoader.Error.handler = (error: any) => {
      console['error'](error)
      ReportFatalError(error.message)
    }
  })
  .catch(err => {
    document.body.classList.remove('dcl-loading')
    if (err.message === AUTH_ERROR_LOGGED_OUT || err.message === NOT_INVITED) {
      ReportFatalError(NOT_INVITED)
    } else {
      console['error']('Error loading Unity', err)
      ReportFatalError(FAILED_FETCHING_UNITY)
    }
  })
