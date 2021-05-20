import { EventConstructor } from '../ecs/EventManager'
import { Observable } from '../ecs/Observable'
import { ReadOnlyQuaternion, ReadOnlyVector3 } from './math'
import { DecentralandInterface, IEvents, RaycastResponsePayload, CameraMode } from './Types'
export { CameraMode }
export { ReadOnlyVector3, ReadOnlyQuaternion }

/**
 * @public
 */
@EventConstructor()
export class UUIDEvent<T = any> {
  constructor(public readonly uuid: string, public readonly payload: T) { }
}

/**
 * @public
 */
@EventConstructor()
export class RaycastResponse<T> {
  constructor(
    public readonly payload: RaycastResponsePayload<T>
  ) { }
}

/**
 * @public
 */
@EventConstructor()
export class PointerEvent<GlobalInputEventResult> {
  constructor(public readonly payload: GlobalInputEventResult) { }
}

let internalDcl: DecentralandInterface | void

/**
 * @internal
 * This function generates a callback that is passed to the Observable
 * constructor to subscribe to the events of the DecentralandInterface
 */
function createSubscriber(eventName: keyof IEvents) {
  return () => {
    if (internalDcl) {
      internalDcl.subscribe(eventName)
    }
  }
}

/**
 * This event is triggered when you change your camera between 1st and 3rd person
 * @public
 */
export const onCameraModeChangedObservable = new Observable<IEvents['cameraModeChanged']>(createSubscriber('cameraModeChanged'))

/**
 * This event is triggered when you change your camera between 1st and 3rd person
 * @public
 */
export const onIdleStateChangedObservable = new Observable<IEvents['idleStateChanged']>(createSubscriber('idleStateChanged'))

/**
 * These events are triggered after your character enters the scene.
 * @public
 */
export const onEnterSceneObservable = new Observable<IEvents['onEnterScene']>(createSubscriber('onEnterScene'))

/* @deprecated Use onEnterSceneObservable instead. */
export const onEnterScene = onEnterSceneObservable

/**
 * These events are triggered after your character leaves the scene.
 * @public
 */
export const onLeaveSceneObservable = new Observable<IEvents['onLeaveScene']>(createSubscriber('onLeaveScene'))

/* @deprecated Use onLeaveSceneObservable instead. */
export const onLeaveScene = onLeaveSceneObservable

/**
 * This event is triggered after all the resources of the scene were loaded (models, textures, etc...)
 * @public
 */
export const onSceneReadyObservable = new Observable<IEvents['sceneStart']>(createSubscriber('sceneStart'))

/**
 * This event is triggered when the position of the camera changes
 * This event is throttled to 10 times per second.
 * @public
 */
export const onPositionChangedObservable = new Observable<IEvents['positionChanged']>(createSubscriber('positionChanged'))

/**
 * This event is triggered when the rotation of the camera changes.
 * This event is throttled to 10 times per second.
 * @public
 */
export const onRotationChangedObservable = new Observable<IEvents['rotationChanged']>(createSubscriber('rotationChanged'))

/**
 * @public
 */
export const onPlayerExpressionObservable = new Observable<IEvents['playerExpression']>(createSubscriber('playerExpression'))

/**
 * @internal
 * This function adds _one_ listener to the onEvent event of dcl interface.
 * Leveraging a switch to route events to the Observable handlers.
 */
export function _initEventObservables(dcl: DecentralandInterface) {
  // store internal reference to dcl, it is going to be used to subscribe to the events
  internalDcl = dcl

  if (internalDcl) {
    internalDcl.onEvent((event) => {
      switch (event.type) {
        case 'onEnterScene': {
          onEnterSceneObservable.notifyObservers(event.data as IEvents['onEnterScene'])
          return
        }
        case 'onLeaveScene': {
          onLeaveSceneObservable.notifyObservers(event.data as IEvents['onLeaveScene'])
          return
        }
        case 'cameraModeChanged': {
          onCameraModeChangedObservable.notifyObservers(event.data as IEvents['cameraModeChanged'])
          return
        }
        case 'idleStateChanged': {
          onIdleStateChangedObservable.notifyObservers(event.data as IEvents['idleStateChanged'])
          return
        }
        case 'sceneStart': {
          onSceneReadyObservable.notifyObservers(event.data as IEvents['sceneStart'])
          return
        }
        case 'positionChanged': {
          onPositionChangedObservable.notifyObservers(event.data as IEvents['positionChanged'])
          return
        }
        case 'rotationChanged': {
          onRotationChangedObservable.notifyObservers(event.data as IEvents['rotationChanged'])
          return
        }
        case 'playerExpression': {
          onPlayerExpressionObservable.notifyObservers(event.data as IEvents['playerExpression'])
          return
        }
      }
    })
  }
}
