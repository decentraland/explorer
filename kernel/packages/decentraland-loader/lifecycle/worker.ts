import { SceneInfoState } from 'decentraland-loader/store/sceneInfo/types'
import { RootState } from 'decentraland-loader/store/state'
import { defaultLogger } from 'shared/logger'
import { WebWorkerTransport } from 'decentraland-rpc'
import { applyMiddleware, compose, createStore, Middleware } from 'redux'
import createSagaMiddleware from 'redux-saga'
import {
  bootstrapAt,
  LoaderAction,
  LOAD_SCENE,
  processUserTeleport,
  PROCESS_PARCEL_SIGHT_CHANGE,
  PROCESS_USER_TELEPORT,
  startScene,
  START_RENDERING,
  START_SCENE,
  STOP_SCENE,
  STORE_RESOLVED_SCENE_ENTITY,
  processUserMovement,
  PROCESS_USER_MOVEMENT
} from '../store/actions'
import { rootReducer } from '../store/reducer'
import { rootSaga } from '../store/saga'
import { Adapter } from './lib/adapter'

const DEBUG_WORKER_LOADER = false
const connector = new Adapter(WebWorkerTransport(self as any))

/**
 * Hook all the events to the connector.
 *
 * Make sure the main thread watches for:
 * - 'Position.settled'
 * - 'Position.unsettled'
 * - 'Scene.shouldStart' (sceneId: string)
 * - 'Scene.shouldUnload' (sceneId: string)
 * - 'Scene.shouldPrefetch' (sceneId: string)
 *
 * Make sure the main thread reports:
 * - 'User.setPosition' { position: {x: number, y: number } }
 * - 'Scene.prefetchDone' { sceneId: string }
 */
{
  connector.on(
    'Lifecycle.initialize',
    (options: {
      contentServer: string
      metaContentServer: string
      contentServerBundles: string
      lineOfSightRadius: number
      secureRadius: number
      emptyScenes: boolean
      tutorialBaseURL: string
      tutorialSceneEnabled: boolean
    }) => {
      const sagaMiddleware = createSagaMiddleware()

      const workerInteractionsMiddleware: Middleware<any, RootState> = api => (next: any) => (action: LoaderAction) => {
        switch (action.type) {
          case START_SCENE:
            break
          case LOAD_SCENE:
            connector.notify('Scene.shouldStart', { sceneId: action.payload })
            break
          case STOP_SCENE:
            connector.notify('Scene.shouldUnload', { sceneId: action.payload })
            break
          case START_RENDERING:
            const spawnPoint = api.getState().position.spawnTarget
            connector.notify('Position.settled', {
              spawnPoint
            })
            break
          case PROCESS_USER_TELEPORT:
            connector.notify('Position.unsettled')
            break
          case PROCESS_PARCEL_SIGHT_CHANGE:
            connector.notify('Sighted', api.getState().sightInfo.recentlySighted)
            connector.notify('Lost sight', api.getState().sightInfo.recentlyLostSight)
            break
          case STORE_RESOLVED_SCENE_ENTITY:
            connector.notify('Scene.dataResponse', {
              data: action.payload.sceneEntity
            })
            connector.notify('Scene.shouldPrefetch', { sceneId: action.payload.sceneEntity.id })
        }
        try {
          if (DEBUG_WORKER_LOADER && action.type !== PROCESS_USER_MOVEMENT) {
            defaultLogger.info(action.type, api.getState())
          }
          return next(action)
        } catch (e) {
          defaultLogger.error('Could not execute action', action, e.stack)
        }
      }
      const store = createStore(
        rootReducer,
        rootReducer(undefined, bootstrapAt('0,0', options)),
        compose(applyMiddleware(sagaMiddleware, workerInteractionsMiddleware))
      )
      sagaMiddleware.run(rootSaga)

      connector.on('User.setPosition', (opt: { position: { x: number; y: number }; teleported: boolean }) => {
        if (opt.teleported) {
          store.dispatch(processUserTeleport(`${opt.position.x},${opt.position.y}`))
        } else {
          if (store.getState().position.isRendering) {
            store.dispatch(processUserMovement(opt.position))
          }
        }
      })

      connector.on('Scene.dataRequest', async (data: { sceneId: string }) => {
        if (store.getState().sceneInfo.sceneIdToSceneJson[data.sceneId]) {
          connector.notify('Scene.dataResponse', {
            data: buildILand(store.getState().sceneInfo, data.sceneId)
          })
        } else {
          const check = () =>
            setTimeout(() => {
              if (store.getState().sceneInfo.sceneIdToSceneJson[data.sceneId]) {
                connector.notify('Scene.dataResponse', {
                  data: buildILand(store.getState().sceneInfo, data.sceneId)
                })
              } else {
                defaultLogger.error('Warning! no data found for ', data.sceneId)
                check()
              }
            }, 1000)
          check()
        }
      })

      connector.on('Scene.prefetchDone', (opt: { sceneId: string }) => {
        // store.dispatch(startScene(opt.sceneId))
      })
      connector.on('Scene.status', (opt: { sceneId: string; status: 'failed' | 'started' }) => {
        // if opt.status === 'failed' set to empty scene or a failure scene
        store.dispatch(startScene(opt.sceneId))
      })
    }
  )
}

function buildILand(sceneInfo: SceneInfoState, sceneId: string) {
  return sceneInfo.sceneIdToSceneJson[sceneId]
}
