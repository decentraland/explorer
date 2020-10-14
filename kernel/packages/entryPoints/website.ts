declare const globalThis: { UnityLoader: any } & StoreContainer
declare const global: any
;(window as any).reactVersion = true

// IMPORTANT! This should be execd before loading 'config' module to ensure that init values are successfully loaded
global.enableWeb3 = true

import { initShared } from 'shared'
import { createLogger } from 'shared/logger'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { AUTH_ERROR_LOGGED_OUT, experienceStarted, FAILED_FETCHING_UNITY, NOT_INVITED } from 'shared/loading/types'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import {
  DEBUG_PM,
  ENABLE_MANA_HUD,
  ENABLE_NEW_TASKBAR,
  HAS_INITIAL_POSITION_MARK,
  NO_MOTD,
  OPEN_AVATAR_EDITOR
} from '../config'
import { signalParcelLoadingStarted, signalRendererInitialized } from 'shared/renderer/actions'
import { lastPlayerPosition, teleportObservable } from 'shared/world/positionThings'
import { RootStore, StoreContainer } from 'shared/store/rootTypes'
import { startUnitySceneWorkers } from '../unity-interface/dcl'
import { initializeUnity, InitializeUnityResult } from '../unity-interface/initializer'
import { HUDElementID } from 'shared/types'
import { onNextWorldRunning, worldRunningObservable } from 'shared/world/worldState'
import { getCurrentIdentity } from 'shared/session/selectors'
import { userAuthentified } from 'shared/session'
import { realmInitialized } from 'shared/dao'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'

const logger = createLogger('website.ts: ')

namespace webApp {
  export function createStore(): RootStore {
    initShared()
    return globalThis.globalStore
  }

  export async function initWeb(container: HTMLElement) {
    if (!container) throw new Error('cannot find element #gameContainer')
    const start = Date.now()
    const observer = worldRunningObservable.add((isRunning) => {
      if (isRunning) {
        worldRunningObservable.remove(observer)
        DEBUG_PM && logger.info(`initial load: `, Date.now() - start)
      }
    })

    return initializeUnity(container).catch((err) => {
      document.body.classList.remove('dcl-loading')
      if (err.message === AUTH_ERROR_LOGGED_OUT || err.message === NOT_INVITED) {
        ReportFatalError(NOT_INVITED)
      } else {
        console['error']('Error loading Unity', err)
        ReportFatalError(FAILED_FETCHING_UNITY)
      }
      throw err
    })
  }

  export async function loadUnity({ instancedJS }: InitializeUnityResult) {
    const i = (await instancedJS).unityInterface

    i.ConfigureHUDElement(HUDElementID.MINIMAP, { active: true, visible: true })
    i.ConfigureHUDElement(
      HUDElementID.PROFILE_HUD,
      { active: true, visible: true },
      { useNewVersion: ENABLE_NEW_TASKBAR }
    )
    i.ConfigureHUDElement(HUDElementID.NOTIFICATION, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.AVATAR_EDITOR, { active: true, visible: OPEN_AVATAR_EDITOR })
    i.ConfigureHUDElement(HUDElementID.SETTINGS, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.EXPRESSIONS, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.PLAYER_INFO_CARD, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.AIRDROPPING, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.TERMS_OF_SERVICE, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.TASKBAR, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.WORLD_CHAT_WINDOW, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.OPEN_EXTERNAL_URL_PROMPT, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.NFT_INFO_DIALOG, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.TELEPORT_DIALOG, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.CONTROLS_HUD, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.EXPLORE_HUD, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.HELP_AND_SUPPORT_HUD, { active: true, visible: false })

    try {
      await userAuthentified()
      const identity = getCurrentIdentity(globalThis.globalStore.getState())!
      i.ConfigureHUDElement(HUDElementID.FRIENDS, { active: identity.hasConnectedWeb3, visible: false })
      i.ConfigureHUDElement(HUDElementID.MANA_HUD, {
        active: ENABLE_MANA_HUD && identity.hasConnectedWeb3,
        visible: true
      })

      if (ENABLE_NEW_TASKBAR) {
        ProfileAsPromise(identity.address)
          .then((profile) => {
            i.ConfigureTutorial(profile.tutorialStep, HAS_INITIAL_POSITION_MARK)
          })
          .catch((e) => logger.error(`error getting profile ${e}`))
      }
    } catch (e) {
      logger.error('error on configuring friends hud')
    }

    globalThis.globalStore.dispatch(signalRendererInitialized())

    onNextWorldRunning(() => globalThis.globalStore.dispatch(experienceStarted()))

    await realmInitialized()
    await startUnitySceneWorkers()

    globalThis.globalStore.dispatch(signalParcelLoadingStarted())

    if (!NO_MOTD) {
      i.ConfigureHUDElement(HUDElementID.MESSAGE_OF_THE_DAY, { active: false, visible: true })
    }

    teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))

    document.body.classList.remove('dcl-loading')
    globalThis.UnityLoader.Error.handler = (error: any) => {
      console['error'](error)
      ReportFatalError(error.message)
    }
    return true
  }
}

global.webApp = webApp
