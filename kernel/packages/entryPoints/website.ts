declare const globalThis: { UnityLoader: any } & StoreContainer
declare const global: any
  ; (window as any).reactVersion = true

// IMPORTANT! This should be execd before loading 'config' module to ensure that init values are successfully loaded
global.enableWeb3 = true

import { initShared } from 'shared'
import { createLogger } from 'shared/logger'
import { BringDownClientAndShowError, ErrorContext, ReportFatalError } from 'shared/loading/ReportFatalError'
import {
  AUTH_ERROR_LOGGED_OUT,
  experienceStarted,
  FAILED_FETCHING_UNITY,
  NOT_INVITED,
  setLoadingScreen,
  setLoadingWaitTutorial
} from 'shared/loading/types'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import { DEBUG_PM, HAS_INITIAL_POSITION_MARK, NO_MOTD, OPEN_AVATAR_EDITOR } from '../config/index'
import { signalParcelLoadingStarted, signalRendererInitialized } from 'shared/renderer/actions'
import { lastPlayerPosition, teleportObservable } from 'shared/world/positionThings'
import { RootStore, StoreContainer } from 'shared/store/rootTypes'
import { trackEvent } from 'shared/analytics'
import { startUnitySceneWorkers } from '../unity-interface/dcl'
import { initializeUnity } from '../unity-interface/initializer'
import { HUDElementID, RenderProfile } from 'shared/types'
import {
  ensureRendererEnabled,
  foregroundObservable,
  isForeground,
  renderStateObservable
} from 'shared/world/worldState'
import { getCurrentIdentity } from 'shared/session/selectors'
import { userAuthentified } from 'shared/session'
import { realmInitialized } from 'shared/dao'
import { EnsureProfile } from 'shared/profiles/ProfileAsPromise'
import { ensureMetaConfigurationInitialized, waitForMessageOfTheDay } from 'shared/meta'
import { FeatureFlags, WorldConfig } from 'shared/meta/types'
import { isFeatureEnabled, isVoiceChatEnabledFor } from 'shared/meta/selectors'
import { UnityInterface } from 'unity-interface/UnityInterface'
import { kernelConfigForRenderer } from '../unity-interface/kernelConfigForRenderer'
import Html from 'shared/Html'
import { filterInvalidNameCharacters, isBadWord } from 'shared/profiles/utils/names'
import { startRealmsReportToRenderer } from 'unity-interface/realmsForRenderer'
import { isWaitingTutorial } from 'shared/loading/selectors'
import { ensureUnityInterface } from 'shared/renderer'

const logger = createLogger('website.ts: ')

function configureTaskbarDependentHUD(i: UnityInterface, voiceChatEnabled: boolean, builderInWorldEnabled: boolean) {
  i.ConfigureHUDElement(
    HUDElementID.TASKBAR,
    { active: true, visible: true },
    {
      enableVoiceChat: voiceChatEnabled,
      enableQuestPanel: isFeatureEnabled(globalThis.globalStore.getState(), FeatureFlags.QUESTS, false)
    }
  )
  i.ConfigureHUDElement(HUDElementID.WORLD_CHAT_WINDOW, { active: true, visible: true })

  i.ConfigureHUDElement(HUDElementID.CONTROLS_HUD, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.EXPLORE_HUD, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.HELP_AND_SUPPORT_HUD, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.BUILDER_PROJECTS_PANEL, { active: builderInWorldEnabled, visible: false })
}

namespace webApp {
  export function createStore(): RootStore {
    initShared()
    return globalThis.globalStore
  }

  export async function initWeb(container: HTMLElement) {
    if (!container) throw new Error('cannot find element #gameContainer')
    const start = Date.now()
    const observer = renderStateObservable.add((isRunning) => {
      if (isRunning) {
        renderStateObservable.remove(observer)
        DEBUG_PM && logger.info(`initial load: `, Date.now() - start)
      }
    })

    try {
      await initializeUnity(container)
      await loadWebsiteSystems()
    } catch (err) {
      document.body.classList.remove('dcl-loading')
      ReportFatalError(err, ErrorContext.WEBSITE_INIT)
      if (err.message === AUTH_ERROR_LOGGED_OUT || err.message === NOT_INVITED) {
        BringDownClientAndShowError(NOT_INVITED)
      } else {
        console['error']('Error loading Unity', err)
        BringDownClientAndShowError(FAILED_FETCHING_UNITY)
      }
      throw err
    }
  }

  async function loadWebsiteSystems() {
    const i = (await ensureUnityInterface()).unityInterface
    const worldConfig: WorldConfig | undefined = globalThis.globalStore.getState().meta.config.world
    const renderProfile = worldConfig ? worldConfig.renderProfile ?? RenderProfile.DEFAULT : RenderProfile.DEFAULT
    const enableNewTutorialCamera = worldConfig ? worldConfig.enableNewTutorialCamera ?? false : false
    const questEnabled = isFeatureEnabled(globalThis.globalStore.getState(), FeatureFlags.QUESTS, false);

    i.ConfigureHUDElement(HUDElementID.MINIMAP, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.NOTIFICATION, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.AVATAR_EDITOR, {
      active: true,
      visible: OPEN_AVATAR_EDITOR
    })
    i.ConfigureHUDElement(HUDElementID.SIGNUP, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.SETTINGS_PANEL, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.EXPRESSIONS, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.PLAYER_INFO_CARD, {
      active: true,
      visible: true
    })
    i.ConfigureHUDElement(HUDElementID.AIRDROPPING, { active: true, visible: true })
    i.ConfigureHUDElement(HUDElementID.TERMS_OF_SERVICE, { active: true, visible: true })

    i.ConfigureHUDElement(HUDElementID.OPEN_EXTERNAL_URL_PROMPT, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.NFT_INFO_DIALOG, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.TELEPORT_DIALOG, { active: true, visible: false })
    i.ConfigureHUDElement(HUDElementID.QUESTS_PANEL, { active: questEnabled, visible: false })
    i.ConfigureHUDElement(HUDElementID.QUESTS_TRACKER, { active: questEnabled, visible: true })

    // NOTE(Brian): Scene download manager uses meta config to determine which empty parcels we want
    //              so ensuring meta configuration is initialized in this stage is a must
    // NOTE(Pablo): We also need meta configuration to know if we need to enable voice chat
    await ensureMetaConfigurationInitialized()

    userAuthentified()
      .then(() => {
        const identity = getCurrentIdentity(globalThis.globalStore.getState())!

        const voiceChatEnabled = isVoiceChatEnabledFor(globalThis.globalStore.getState(), identity.address)
        const builderInWorldEnabled =
          identity.hasConnectedWeb3 &&
          isFeatureEnabled(globalThis.globalStore.getState(), FeatureFlags.BUILDER_IN_WORLD, false)

        const configForRenderer = kernelConfigForRenderer()
        configForRenderer.comms.voiceChatEnabled = voiceChatEnabled
        configForRenderer.features.enableBuilderInWorld = builderInWorldEnabled
        i.SetKernelConfiguration(configForRenderer)

        configureTaskbarDependentHUD(i, voiceChatEnabled, builderInWorldEnabled)

        i.ConfigureHUDElement(HUDElementID.PROFILE_HUD, { active: true, visible: true })
        i.ConfigureHUDElement(HUDElementID.USERS_AROUND_LIST_HUD, { active: voiceChatEnabled, visible: false })
        i.ConfigureHUDElement(HUDElementID.FRIENDS, { active: identity.hasConnectedWeb3, visible: false })

        ensureRendererEnabled()
          .then(() => {
            globalThis.globalStore.dispatch(setLoadingWaitTutorial(false))
            globalThis.globalStore.dispatch(experienceStarted())
            globalThis.globalStore.dispatch(setLoadingScreen(false))
            Html.switchGameContainer(true)
          })
          .catch(logger.error)

        const tutorialConfig = {
          fromDeepLink: HAS_INITIAL_POSITION_MARK,
          enableNewTutorialCamera: enableNewTutorialCamera
        }

        EnsureProfile(identity.address)
          .then((profile) => {
            i.ConfigureTutorial(profile.tutorialStep, tutorialConfig)
            i.ConfigureHUDElement(HUDElementID.GRAPHIC_CARD_WARNING, { active: true, visible: true })

            // NOTE: here we make sure that if signup (tutorial) just finished
            // the player is set to the correct spawn position plus we make sure that the proper scene is loaded
            setUserPositionAfterTutorial()
          })
          .catch((e) => logger.error(`error getting profile ${e}`))
      })
      .catch((e) => {
        logger.error('error on configuring taskbar & friends hud / tutorial. Trying to default to simple taskbar', e)
        configureTaskbarDependentHUD(i, false, false)
      })

    globalThis.globalStore.dispatch(signalRendererInitialized())

    await realmInitialized()
    startRealmsReportToRenderer()

    await startUnitySceneWorkers()

    globalThis.globalStore.dispatch(signalParcelLoadingStarted())

    i.SetRenderProfile(renderProfile)

    if (isForeground()) {
      i.ReportFocusOn()
    } else {
      i.ReportFocusOff()
    }

    foregroundObservable.add((isForeground) => {
      if (isForeground) {
        i.ReportFocusOn()
      } else {
        i.ReportFocusOff()
      }
    })

    if (!NO_MOTD) {
      waitForMessageOfTheDay()
        .then((messageOfTheDay) => {
          i.ConfigureHUDElement(
            HUDElementID.MESSAGE_OF_THE_DAY,
            { active: !!messageOfTheDay, visible: false },
            messageOfTheDay
          )
        })
        .catch(() => {
          /*noop*/
        })
    }

    teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))

    document.body.classList.remove('dcl-loading')

    return true
  }

  // This is for shared functionality between kernel and website.
  // This is not very good because we can't type check it.
  // In the future, we should probably replace this with a library
  export const utils = {
    isBadWord,
    filterInvalidNameCharacters,
    trackEvent
  }

  function setUserPositionAfterTutorial() {
    if (isWaitingTutorial(globalThis.globalStore.getState())) {
      teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))
    }
  }
}

global.webApp = webApp
