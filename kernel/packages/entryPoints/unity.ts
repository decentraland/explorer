declare const globalThis: { UnityLoader: any } & StoreContainer
declare const global: any

// IMPORTANT! This should be execd before loading 'config' module to ensure that init values are successfully loaded
global.enableWeb3 = true

import { createLogger } from 'shared/logger'
import { ReportFatalError } from 'shared/loading/ReportFatalError'
import { experienceStarted, NOT_INVITED, AUTH_ERROR_LOGGED_OUT, FAILED_FETCHING_UNITY } from 'shared/loading/types'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import {
  NO_MOTD,
  DEBUG_PM,
  HAS_INITIAL_POSITION_MARK,
} from '../config/index'
import { signalRendererInitialized, signalParcelLoadingStarted } from 'shared/renderer/actions'
import { lastPlayerPosition, teleportObservable } from 'shared/world/positionThings'
import { StoreContainer } from 'shared/store/rootTypes'
import { startUnitySceneWorkers } from '../unity-interface/dcl'
import { initializeUnity } from '../unity-interface/initializer'
import { HUDElementID } from 'shared/types'
import { worldRunningObservable, onNextWorldRunning } from 'shared/world/worldState'
import { getCurrentIdentity } from 'shared/session/selectors'
import { userAuthentified } from 'shared/session'
import { realmInitialized } from 'shared/dao'
import { EnsureProfile } from 'shared/profiles/ProfileAsPromise'

const container = document.getElementById('gameContainer')
const qs: any = require('query-string')

if (!container) throw new Error('cannot find element #gameContainer')

const logger = createLogger('unity.ts: ')

const start = Date.now()

const observer = worldRunningObservable.add((isRunning) => {
  if (isRunning) {
    worldRunningObservable.remove(observer)
    DEBUG_PM && logger.info(`initial load: `, Date.now() - start)
  }
})

initializeUnity(container)
  .then(async ({ instancedJS }) => {
    const i = (await instancedJS).unityInterface

    const q = qs.parse(location.search)

    const enableUI = q.enable_ui
    const enableTutorialUI = q.enable_tutorial_ui
    const enableWeb3UI = q.enable_web3_ui
    
    logger.info( `enableUI = ${enableUI}`)

    if ( enableUI ) {
      i.ConfigureHUDElement(enableUI, { active: true, visible: true })
    } 
    
    try {
      await userAuthentified()
      const identity = getCurrentIdentity(globalThis.globalStore.getState())!

      if (enableWeb3UI) {
        i.ConfigureHUDElement(HUDElementID.FRIENDS, { active: identity.hasConnectedWeb3, visible: false })
        i.ConfigureHUDElement(HUDElementID.MANA_HUD, { active: identity.hasConnectedWeb3, visible: true })
      }
      
      EnsureProfile(identity.address)
          .then((profile) => {
            if ( enableTutorialUI ) {
              i.ConfigureEmailPrompt(profile.tutorialStep)
              i.ConfigureTutorial(profile.tutorialStep, HAS_INITIAL_POSITION_MARK)
            }
          })
          .catch((e) => logger.error(`error getting profile ${e}`))
    } catch (e) {
      logger.error('error on configuring friends hud / tutorial')
    }

    globalThis.globalStore.dispatch(signalRendererInitialized())

    onNextWorldRunning(() => globalThis.globalStore.dispatch(experienceStarted()))

    await realmInitialized()
    await startUnitySceneWorkers()

    globalThis.globalStore.dispatch(signalParcelLoadingStarted())

    if (!NO_MOTD && !enableUI) {
      i.ConfigureHUDElement(HUDElementID.MESSAGE_OF_THE_DAY, { active: false, visible: true })
    }

    teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))

    document.body.classList.remove('dcl-loading')
    globalThis.UnityLoader.Error.handler = (error: any) => {
      console['error'](error)
      ReportFatalError(error.message)
    }
  })
  .catch((err) => {
    document.body.classList.remove('dcl-loading')
    if (err.message === AUTH_ERROR_LOGGED_OUT || err.message === NOT_INVITED) {
      ReportFatalError(NOT_INVITED)
    } else {
      console['error']('Error loading Unity', err)
      ReportFatalError(FAILED_FETCHING_UNITY)
    }
  })
