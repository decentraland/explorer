import { call, put, select, take, takeLatest } from 'redux-saga/effects'
import { ETHEREUM_NETWORK, FORCE_RENDERING_STYLE, getAssetBundlesBaseUrl, getServerConfigurations } from 'config'
import { META_CONFIGURATION_INITIALIZED, metaConfigurationInitialized, metaUpdateMessageOfTheDay } from './actions'
import defaultLogger from '../logger'
import { buildNumber } from './env'
import { BannedUsers, MetaConfiguration, WorldConfig } from './types'
import { isMetaConfigurationInitiazed } from './selectors'
import { USER_AUTHENTIFIED } from '../session/actions'
import { getCurrentUserId } from '../session/selectors'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { SELECT_NETWORK } from 'shared/dao/actions'

function bannedUsersFromVariants(variants: Record<string, any> | undefined): BannedUsers | undefined {
  const variant = variants?.['explorer-banned_users']
  if (variant && variant.enabled) {
    try {
      return JSON.parse(variant.payload.value)
    } catch (e) {
      defaultLogger.warn("Couldn't parse banned users from variants. The variants response was: ", variants)
    }
  }
}

export function* metaSaga(): any {
  yield take(SELECT_NETWORK)

  const net: ETHEREUM_NETWORK = yield select(getSelectedNetwork)
  const config: Partial<MetaConfiguration> = yield call(fetchMetaConfiguration, net)
  const flagsAndVariants: { flags: Record<string, boolean>; variants: Record<string, any> } | undefined = yield call(
    fetchFeatureFlagsAndVariants,
    net
  )
  const merge: Partial<MetaConfiguration> = {
    ...config,
    featureFlags: flagsAndVariants?.flags,
    bannedUsers: bannedUsersFromVariants(flagsAndVariants?.variants)
  }

  if (FORCE_RENDERING_STYLE) {
    if (!merge.world) {
      merge.world = {} as WorldConfig
    }

    merge.world.renderProfile = FORCE_RENDERING_STYLE
  }

  yield put(metaConfigurationInitialized(merge))
  yield call(checkExplorerVersion, merge)
  yield takeLatest(USER_AUTHENTIFIED, fetchMessageOfTheDay)
}

function* fetchMessageOfTheDay() {
  const userId: string | undefined = yield select(getCurrentUserId)
  if (userId) {
    const url = `https://dclcms.club/api/notifications?address=${userId}`
    const result = yield call(async () => {
      try {
        const response = await fetch(url)
        const data = await response.json()
        return data?.length ? data[0] : null
      } catch (e) {
        defaultLogger.error(`Error fetching Message of the day ${e}`)
        return null
      }
    })
    yield put(metaUpdateMessageOfTheDay(result))
  }
}

function checkExplorerVersion(config: Partial<MetaConfiguration>) {
  const currentBuildNumber = buildNumber
  defaultLogger.info(`Current build number: `, currentBuildNumber)

  if (!config || !config.explorer || !config.explorer.minBuildNumber) {
    return
  }

  if (currentBuildNumber < config.explorer.minBuildNumber) {
    // force client to reload from server
    window.location.reload(true)
  }
}

async function fetchFeatureFlagsAndVariants(network: ETHEREUM_NETWORK): Promise<Record<string, boolean> | undefined> {
  const featureFlagsEndpoint = getServerConfigurations(network).explorerFeatureFlags
  try {
    const response = await fetch(featureFlagsEndpoint, {
      credentials: 'include'
    })
    if (response.ok) {
      return response.json()
    }
  } catch (e) {
    defaultLogger.warn(`Error while fetching feature flags from '${featureFlagsEndpoint}'. Using default config`)
  }
}

async function fetchMetaConfiguration(network: ETHEREUM_NETWORK) {
  const explorerConfigurationEndpoint = getServerConfigurations(network).explorerConfiguration
  try {
    const response = await fetch(explorerConfigurationEndpoint)
    if (response.ok) {
      return response.json()
    }
    throw new Error('Meta Response Not Ok')
  } catch (e) {
    defaultLogger.warn(
      `Error while fetching meta configuration from '${explorerConfigurationEndpoint}' using default config`
    )
    return {
      explorer: {
        minBuildNumber: 0,
        assetBundlesFetchUrl: getAssetBundlesBaseUrl(network)
      },
      servers: {
        added: [],
        denied: [],
        contentWhitelist: []
      },
      bannedUsers: {},
      synapseUrl: 'https://synapse.decentraland.org',
      world: {
        pois: []
      },
      comms: {
        targetConnections: 4,
        maxConnections: 6
      }
    }
  }
}

export function* waitForMetaConfigurationInitialization() {
  if (!(yield select(isMetaConfigurationInitiazed))) {
    yield take(META_CONFIGURATION_INITIALIZED)
  }
}
