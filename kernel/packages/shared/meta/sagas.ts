import { call, put, select, take, takeLatest } from 'redux-saga/effects'
import { getServerConfigurations, HALLOWEEN } from 'config'
import { META_CONFIGURATION_INITIALIZED, metaConfigurationInitialized, metaUpdateMessageOfTheDay } from './actions'
import defaultLogger from '../logger'
import { buildNumber } from './env'
import { MetaConfiguration, USE_UNITY_INDEXED_DB_CACHE, WorldConfig } from './types'
import { isMetaConfigurationInitiazed } from './selectors'
import { RenderProfile } from 'shared/types'
import { USER_AUTHENTIFIED } from '../session/actions'

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
  yield takeLatest(USER_AUTHENTIFIED, fetchMessageOfTheDay)
}

function* fetchMessageOfTheDay() {
  const userId = yield select((state) => state.session.userId)
  console.log('result-day-user:', userId)
  const url = 'https://dclcms.club/api/notifications?address='
  const result = yield call(() =>
    fetch(url + userId)
      .then((response) => response.json())
      .then((data) => {
        // console.log('result-day-data: ', data)
        // return data[0]
        return {
          background_banner: 'http://dclcms.club/media/background-images/MOTDHeader.png',
          body:
            'Welcome to Halloween!\n\nLorem ipsum dolor sit amet, consectetur adipiscing elit. Aliquam pretium congue orci, sit amet aliquet purus fermentum nec. Nunc finibus turpis quis facilisis laoreet. Sed nec hendrerit augue, in finibus ipsum. Curabitur semper urna lectus. Sed fermentum ex commodo nisl maximus bibendum. Integer ut neque quis dolor porta pharetra eu vitae massa. Nam feugiat et risus a pellentesque. Maecenas fermentum arcu id quam scelerisque, ac sodales lacus consequat. Nullam posuere eleifend ipsum quis vehicula. Fusce ipsum nisl, accumsan ac lectus et, hendrerit consequat felis.',
          title: 'Halloween Day 1',
          buttons: [{ caption: 'Continue', tint: { r: 0, g: 0, b: 1, a: 1 } }]
        }
      })
  )
  // console.log('result-day-result-2: ', result)
  console.log('result-day-result: ', result)
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
