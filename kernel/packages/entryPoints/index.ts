declare const globalThis: StoreContainer & { DecentralandKernel: IDecentralandKernel }

import defaultLogger, { createLogger } from 'shared/logger'
import type { IDecentralandKernel, KernelOptions, KernelResult } from '@dcl/kernel-interface'
import { BringDownClientAndShowError, ErrorContext, ReportFatalError } from 'shared/loading/ReportFatalError'
import {
  AUTH_ERROR_LOGGED_OUT,
  FAILED_FETCHING_UNITY,
  NOT_INVITED,
  renderingInBackground,
  renderingInForeground
  // setLoadingWaitTutorial
} from 'shared/loading/types'
import { worldToGrid } from '../atomicHelpers/parcelScenePositions'
import { DEBUG_WS_MESSAGES, HAS_INITIAL_POSITION_MARK, OPEN_AVATAR_EDITOR } from '../config/index'
import { signalParcelLoadingStarted } from 'shared/renderer/actions'
import { lastPlayerPosition, pickWorldSpawnpoint, teleportObservable } from 'shared/world/positionThings'
import { StoreContainer } from 'shared/store/rootTypes'
import { loadPreviewScene, startUnitySceneWorkers } from '../unity-interface/dcl'
import { initializeUnity } from '../unity-interface/initializer'
import { HUDElementID, ILand, RenderProfile } from 'shared/types'
import { foregroundChangeObservable, isForeground } from 'shared/world/worldState'
import { getCurrentIdentity } from 'shared/session/selectors'
import { authenticateWhenItsReady, userAuthentified } from 'shared/session'
import { realmInitialized } from 'shared/dao'
import { EnsureProfile } from 'shared/profiles/ProfileAsPromise'
import { ensureMetaConfigurationInitialized, waitForMessageOfTheDay } from 'shared/meta'
import { FeatureFlags, WorldConfig } from 'shared/meta/types'
import { isFeatureEnabled, isVoiceChatEnabledFor } from 'shared/meta/selectors'
import { UnityInterface } from 'unity-interface/UnityInterface'
import { kernelConfigForRenderer } from '../unity-interface/kernelConfigForRenderer'
import { startRealmsReportToRenderer } from 'unity-interface/realmsForRenderer'
import { isWaitingTutorial } from 'shared/loading/selectors'
import { ensureUnityInterface } from 'shared/renderer'
import {
  errorObservable,
  loadingProgressObservable,
  trackingEventObservable,
  accountStateObservable,
  signUpObservable,
  rendererVisibleObservable,
  openUrlObservable
} from 'shared/observables'
import { initShared } from 'shared'
import { sceneLifeCycleObservable } from 'decentraland-loader/lifecycle/controllers/scene'
import future, { IFuture } from 'fp-future'
import { setResourcesURL } from 'shared/location'
import { WebSocketProvider } from 'eth-connect'
import { resolveUrlFromUrn } from '@dcl/urn-resolver'

const logger = createLogger('kernel: ')

function configureTaskbarDependentHUD(i: UnityInterface, voiceChatEnabled: boolean, builderInWorldEnabled: boolean) {
  // The elements below, require the taskbar to be active before being activated.

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

async function resolveBaseUrl(urn: string): Promise<string> {
  if (urn.startsWith('urn:')) {
    const t = await resolveUrlFromUrn(urn)
    if (t) {
      return (t + '/').replace(/(\/)+$/, '/')
    }
    throw new Error('Cannot resolve content for URN ' + urn)
  }
  return (urn + '/').replace(/(\/)+$/, '/')
}

function orFail(withError: string): never {
  throw new Error(withError)
}

globalThis.DecentralandKernel = {
  async initKernel(options: KernelOptions): Promise<KernelResult> {
    options.kernelOptions.baseUrl = await resolveBaseUrl(
      options.kernelOptions.baseUrl || orFail('MISSING kernelOptions.baseUrl')
    )
    options.rendererOptions.baseUrl = await resolveBaseUrl(
      options.rendererOptions.baseUrl || orFail('MISSING rendererOptions.baseUrl')
    )

    const { container } = options.rendererOptions
    const { baseUrl } = options.kernelOptions

    if (baseUrl) {
      setResourcesURL(baseUrl)
    }

    if (!container) throw new Error('cannot find element #gameContainer')

    initShared()

    Promise.resolve()
      .then(() => initializeUnity(options.rendererOptions))
      .then(() => loadWebsiteSystems(options.kernelOptions))
      .catch((err) => {
        ReportFatalError(err, ErrorContext.WEBSITE_INIT)
        if (err.message === AUTH_ERROR_LOGGED_OUT || err.message === NOT_INVITED) {
          BringDownClientAndShowError(NOT_INVITED)
        } else {
          BringDownClientAndShowError(FAILED_FETCHING_UNITY)
        }
      })

    return {
      authenticate(provider: any, isGuest: boolean) {
        if (!provider) {
          throw new Error('A provider must be provided')
        }
        if (typeof provider === 'string') {
          if (provider.startsWith('ws:') || provider.startsWith('wss:')) {
            provider = new WebSocketProvider(provider)
          } else {
            throw new Error('Text provider can only be WebSocket')
          }
        }
        authenticateWhenItsReady(provider, isGuest)
      },
      accountStateObservable,
      errorObservable,
      loadingProgressObservable,
      trackingEventObservable,
      signUpObservable,
      rendererVisibleObservable,
      openUrlObservable,
      version: 'mockedversion'
    }
  }
}

async function loadWebsiteSystems(options: KernelOptions['kernelOptions']) {
  const i = (await ensureUnityInterface()).unityInterface

  // NOTE(Brian): Scene download manager uses meta config to determine which empty parcels we want
  //              so ensuring meta configuration is initialized in this stage is a must
  // NOTE(Pablo): We also need meta configuration to know if we need to enable voice chat
  await ensureMetaConfigurationInitialized()

  const worldConfig: WorldConfig | undefined = globalThis.globalStore.getState().meta.config.world
  const renderProfile = worldConfig ? worldConfig.renderProfile ?? RenderProfile.DEFAULT : RenderProfile.DEFAULT
  i.SetRenderProfile(renderProfile)
  const enableNewTutorialCamera = worldConfig ? worldConfig.enableNewTutorialCamera ?? false : false
  const questEnabled = isFeatureEnabled(globalThis.globalStore.getState(), FeatureFlags.QUESTS, false)

  i.ConfigureHUDElement(HUDElementID.MINIMAP, { active: true, visible: true })
  i.ConfigureHUDElement(HUDElementID.NOTIFICATION, { active: true, visible: true })
  i.ConfigureHUDElement(HUDElementID.AVATAR_EDITOR, { active: true, visible: OPEN_AVATAR_EDITOR })
  i.ConfigureHUDElement(HUDElementID.SIGNUP, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.LOADING_HUD, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.SETTINGS_PANEL, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.EXPRESSIONS, { active: true, visible: true })
  i.ConfigureHUDElement(HUDElementID.PLAYER_INFO_CARD, { active: true, visible: true })
  i.ConfigureHUDElement(HUDElementID.AIRDROPPING, { active: true, visible: true })
  i.ConfigureHUDElement(HUDElementID.TERMS_OF_SERVICE, { active: true, visible: true })
  i.ConfigureHUDElement(HUDElementID.OPEN_EXTERNAL_URL_PROMPT, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.NFT_INFO_DIALOG, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.TELEPORT_DIALOG, { active: true, visible: false })
  i.ConfigureHUDElement(HUDElementID.QUESTS_PANEL, { active: questEnabled, visible: false })
  i.ConfigureHUDElement(HUDElementID.QUESTS_TRACKER, { active: questEnabled, visible: true })

  userAuthentified()
    .then(() => {
      const identity = getCurrentIdentity(globalThis.globalStore.getState())!

      const VOICE_CHAT_ENABLED = isVoiceChatEnabledFor(globalThis.globalStore.getState(), identity.address)
      const BUILDER_IN_WORLD_ENABLED =
        identity.hasConnectedWeb3 &&
        isFeatureEnabled(globalThis.globalStore.getState(), FeatureFlags.BUILDER_IN_WORLD, false)

      const configForRenderer = kernelConfigForRenderer()
      configForRenderer.comms.voiceChatEnabled = VOICE_CHAT_ENABLED
      configForRenderer.features.enableBuilderInWorld = BUILDER_IN_WORLD_ENABLED
      i.SetKernelConfiguration(configForRenderer)

      configureTaskbarDependentHUD(i, VOICE_CHAT_ENABLED, BUILDER_IN_WORLD_ENABLED)

      i.ConfigureHUDElement(HUDElementID.PROFILE_HUD, { active: true, visible: true })
      i.ConfigureHUDElement(HUDElementID.USERS_AROUND_LIST_HUD, { active: VOICE_CHAT_ENABLED, visible: false })
      i.ConfigureHUDElement(HUDElementID.FRIENDS, { active: identity.hasConnectedWeb3, visible: false })

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
          if (isWaitingTutorial(globalThis.globalStore.getState())) {
            teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))
          }
        })
        .catch((e) => logger.error(`error getting profile ${e}`))
    })
    .catch((e) => {
      logger.error('error on configuring taskbar & friends hud / tutorial. Trying to default to simple taskbar', e)
      configureTaskbarDependentHUD(i, false, false)
    })

  await realmInitialized()
  startRealmsReportToRenderer()

  globalThis.globalStore.dispatch(signalParcelLoadingStarted())

  function reportForeground() {
    if (isForeground()) {
      globalThis.globalStore.dispatch(renderingInForeground())
      i.ReportFocusOn()
    } else {
      globalThis.globalStore.dispatch(renderingInBackground())
      i.ReportFocusOff()
    }
  }

  foregroundChangeObservable.add(reportForeground)
  reportForeground()

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

  if (options.previewMode) {
    const scene = await startPreview()
    const position = pickWorldSpawnpoint(scene)
    i.Teleport(position)
    teleportObservable.notifyObservers(position.position)
  } else {
    await startUnitySceneWorkers()
    teleportObservable.notifyObservers(worldToGrid(lastPlayerPosition))
  }

  document.body.classList.remove('dcl-loading')

  return true
}

async function startPreview() {
  function sceneRenderable() {
    const sceneRenderable = future<void>()

    const observer = sceneLifeCycleObservable.add(async (sceneStatus) => {
      if (sceneStatus.sceneId === (await defaultScene).sceneId) {
        sceneLifeCycleObservable.remove(observer)
        sceneRenderable.resolve()
      }
    })

    return sceneRenderable
  }

  const defaultScene: IFuture<ILand> = future()

  let wsScene: string | undefined = undefined

  if (location.search.indexOf('WS_SCENE') !== -1) {
    wsScene = `${location.protocol === 'https:' ? 'wss' : 'ws'}://${document.location.host}/?scene`
  }

  function startSceneLoading() {
    // this is set to avoid double loading scenes due queued messages
    let isSceneLoading: boolean = true

    const loadScene = () => {
      isSceneLoading = true
      loadPreviewScene(wsScene)
        .then((scene) => {
          isSceneLoading = false
          defaultScene.resolve(scene)
        })
        .catch((err) => {
          isSceneLoading = false
          defaultLogger.error('Error loading scene', err)
          defaultScene.reject(err)
        })
    }

    loadScene()
    ;(globalThis as any).handleServerMessage = function (message: any) {
      if (message.type === 'update') {
        if (DEBUG_WS_MESSAGES) {
          defaultLogger.info('Message received: ', message)
        }
        // if a scene is currently loading we do not trigger another load
        if (isSceneLoading) {
          if (DEBUG_WS_MESSAGES) {
            defaultLogger.trace('Ignoring message, scene still loading...')
          }
          return
        }

        loadScene()
      }
    }
  }

  await sceneRenderable()
  startSceneLoading()

  return defaultScene
}
