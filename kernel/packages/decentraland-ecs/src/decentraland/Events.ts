import { EventConstructor } from '../ecs/EventManager'

/**
 * @public
 */
@EventConstructor()
export class UUIDEvent<T = any> {
  constructor(public readonly uuid: string, public readonly payload: T) {}
}

/**
 * @public
 */
@EventConstructor()
export class RaycastResponse<T> {
  constructor(
    public readonly payload: {
      queryId: string
      queryType: string
      payload: T
    }
  ) {}
}

/**
 * @public
 */
@EventConstructor()
export class PointerEvent<GlobalInputEventResult> {
  constructor(public readonly payload: GlobalInputEventResult) {}
}
