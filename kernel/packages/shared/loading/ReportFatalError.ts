declare var window: Window & { Rollbar: any }

import {
  COMMS_COULD_NOT_BE_ESTABLISHED,
  fatalError,
  ExecutionLifecycleEvent,
  MOBILE_NOT_SUPPORTED,
  NETWORK_MISMATCH,
  NEW_LOGIN,
  NO_WEBGL_COULD_BE_CREATED,
  NOT_INVITED,
  AVATAR_LOADING_ERROR,
  ExecutionLifecycleEventsList
} from './types'
import { trackEvent } from '../analytics'
import { action } from 'typesafe-actions'
import { globalObservable } from '../observables'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { store } from 'shared/store/isolatedStore'

export function BringDownClientAndShowError(event: ExecutionLifecycleEvent) {
  if (ExecutionLifecycleEventsList.includes(event)) {
    store.dispatch(action(event))
  }

  const targetError =
    event === COMMS_COULD_NOT_BE_ESTABLISHED
      ? 'comms'
      : event === NOT_INVITED
      ? 'notinvited'
      : event === NO_WEBGL_COULD_BE_CREATED
      ? 'notsupported'
      : event === MOBILE_NOT_SUPPORTED
      ? 'nomobile'
      : event === NEW_LOGIN
      ? 'newlogin'
      : event === NETWORK_MISMATCH
      ? 'networkmismatch'
      : event === AVATAR_LOADING_ERROR
      ? 'avatarerror'
      : 'fatal'

  store.dispatch(fatalError(targetError))

  globalObservable.emit('error', {
    error: new Error(event),
    code: targetError,
    level: 'fatal'
  })
}

export namespace ErrorContext {
  export const WEBSITE_INIT = `website#init`
  export const COMMS_INIT = `comms#init`
  export const KERNEL_INIT = `kernel#init`
  export const KERNEL_SAGA = `kernel#saga`
  export const KERNEL_SCENE = `kernel#scene`
  export const RENDERER_AVATARS = `renderer#avatars`
  export const RENDERER_ERRORHANDLER = `renderer#errorHandler`
}

export type ErrorContextTypes =
  | typeof ErrorContext.WEBSITE_INIT
  | typeof ErrorContext.COMMS_INIT
  | typeof ErrorContext.KERNEL_INIT
  | typeof ErrorContext.KERNEL_SAGA
  | typeof ErrorContext.KERNEL_SCENE
  | typeof ErrorContext.RENDERER_AVATARS
  | typeof ErrorContext.RENDERER_ERRORHANDLER

export function ReportFatalErrorWithCatalystPayload(error: Error, context: ErrorContextTypes) {
  // TODO(Brian): Get some useful catalyst payload to append here
  ReportFatalError(error, context)
}

export function ReportFatalErrorWithCommsPayload(error: Error, context: ErrorContextTypes) {
  // TODO(Brian): Get some useful comms payload to append here
  ReportFatalError(error, context)
}

export function ReportFatalErrorWithUnityPayload(error: Error, context: ErrorContextTypes) {
  ReportFatalErrorWithUnityPayloadAsync(error, context)
    .then((x) => {
      //
    })
    .catch(() => {
      //
    })
}

export async function ReportFatalErrorWithUnityPayloadAsync(error: Error, context: ErrorContextTypes) {
  try {
    let payload = await getUnityInstance().CrashPayloadRequest()
    ReportFatalError(error, context, { rendererPayload: payload })
  } catch (e) {
    ReportFatalError(error, context)
  }
}

export function ReportFatalError(error: Error, context: ErrorContextTypes, payload: Record<string, any> = {}) {
  let sagaStack: string | undefined = payload['sagaStack']

  if (sagaStack) {
    // first stringify
    sagaStack = '' + sagaStack
    // then crop
    sagaStack = sagaStack.slice(0, 10000)
  }

  // segment requires less information than rollbar
  trackEvent('error_fatal', {
    context,
    // this is on purpose, if error is not an actual Error, it has no message, so we use the ''+error to call a
    // toString, we do that because it may be also null. and (null).toString() is invalid, but ''+null works perfectly
    message: error.message || '' + error,
    stack: getStack(error).slice(0, 10000),
    saga_stack: sagaStack
  })

  globalObservable.emit('error', {
    error,
    level: 'fatal',
    extra: { context, ...payload }
  })
}

function getStack(error?: any) {
  if (error && error.stack) {
    return error.stack
  } else {
    try {
      throw new Error((error && error.message) || error || '<nullish error>')
    } catch (e) {
      return e.stack || '' + error
    }
  }
}
