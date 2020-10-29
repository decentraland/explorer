import { call, put, select, take, takeLatest } from 'redux-saga/effects'
import { getServerConfigurations, HALLOWEEN } from 'config'
import { META_CONFIGURATION_INITIALIZED, metaConfigurationInitialized, metaUpdateMessageOfTheDay } from './actions'
import defaultLogger from '../logger'
import { buildNumber } from './env'
import { MetaConfiguration, USE_UNITY_INDEXED_DB_CACHE, WorldConfig } from './types'
import { isMetaConfigurationInitiazed } from './selectors'
import { RenderProfile } from 'shared/types'
import { USER_AUTHENTIFIED } from '../session/actions'
import { getUserId } from '../session/selectors'

const DEFAULT_META_CONFIGURATION: MetaConfiguration = {
  explorer: {
    minBuildNumber: 0,
    useUnityIndexedDbCache: false
  },
  servers: {
    added: [],
    denied: [],
    contentWhitelist: []
  },
  world: {
    pois: []
  },
  comms: {
    targetConnections: 4,
    maxConnections: 6
  }
}

export function* metaSaga(): any {
  const config: Partial<MetaConfiguration> = yield call(fetchMetaConfiguration)

  if (HALLOWEEN) {
    if (!config.world) {
      config.world = {} as WorldConfig
    }

    config.world.renderProfile = HALLOWEEN ? RenderProfile.HALLOWEEN : RenderProfile.DEFAULT
  }

  yield put(metaConfigurationInitialized(config))
  yield call(checkExplorerVersion, config)
  yield call(checkIndexedDB, config)
  const userId = yield select(getUserId)
  if (userId) {
    yield call(fetchMessageOfTheDay)
  } else {
    yield takeLatest(USER_AUTHENTIFIED, fetchMessageOfTheDay)
  }
}

function* fetchMessageOfTheDay() {
  const userId = yield select((state) => state.session.userId)
  const url = `https://dclcms.club/api/notifications?address=${userId}`
  const result = yield call(() =>
    fetch(url)
      .then((response) => {
        return response.json()
      })
      .then((data) => (data.length ? data[0] : null))
  )
  yield put(metaUpdateMessageOfTheDay(result))
}

function checkIndexedDB(config: Partial<MetaConfiguration>) {
  if (!config || !config.explorer) {
    return
  }

  if (!config.explorer.useUnityIndexedDbCache) {
    defaultLogger.info(`Unity IndexedDB meta config is undefined. Defaulting as false (only for chrome)`)
    USE_UNITY_INDEXED_DB_CACHE.resolve(false)
    return
  }

  defaultLogger.info(
    `Unity IndexedDB meta config loaded. Configured remotely as: `,
    config.explorer.useUnityIndexedDbCache
  )
  USE_UNITY_INDEXED_DB_CACHE.resolve(config.explorer.useUnityIndexedDbCache)
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

async function fetchMetaConfiguration() {
  const explorerConfigurationEndpoint = getServerConfigurations().explorerConfiguration
  try {
    const response = await fetch(explorerConfigurationEndpoint)
    return response.ok ? response.json() : DEFAULT_META_CONFIGURATION
  } catch (e) {
    defaultLogger.warn(
      `Error while fetching meta configuration from '${explorerConfigurationEndpoint}' using default config`
    )
    return DEFAULT_META_CONFIGURATION
  }
}

export function* waitForMetaConfigurationInitialization() {
  if (!(yield select(isMetaConfigurationInitiazed))) {
    yield take(META_CONFIGURATION_INITIALIZED)
  }
}
