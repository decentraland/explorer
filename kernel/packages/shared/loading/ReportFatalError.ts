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
import { StoreContainer } from 'shared/store/rootTypes'
import Html from '../Html'
import { trackEvent } from '../analytics'
import { action } from 'typesafe-actions'
import { unityInterface } from 'unity-interface/UnityInterface'

declare const globalThis: StoreContainer

export let aborted = false

export function BringDownClientAndShowError(event: ExecutionLifecycleEvent) {
  if (aborted) {
    return
  }

  if (ExecutionLifecycleEventsList.includes(event)) {
    globalThis.globalStore.dispatch(action(event))
  }

  const body = document.body
  const container = document.getElementById('gameContainer')
  container!.setAttribute('style', 'display: none !important')

  Html.hideProgressBar()

  body.setAttribute('style', 'background-image: none !important;')

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

  globalThis.globalStore && globalThis.globalStore.dispatch(fatalError(targetError))
  Html.showErrorModal(targetError)
  aborted = true
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
  unityInterface
    .CrashPayloadRequest()
    .then((payload) => {
      ReportFatalError(error, context, JSON.parse(payload))
    })
    .catch(() => {
      ReportFatalError(error, context)
    })
}

export function ReportFatalError(error: Error, context: ErrorContextTypes, payload: any = null) {
  const finalPayload = GetErrorPayload(context, payload)
  trackEvent('error_generic', {
    message: error.message,
    payload: finalPayload
  })

  ReportRollbarError(error, finalPayload)
}

export function ReportSceneError(message: string, payload: any) {
  const finalPayload = GetErrorPayload(ErrorContext.KERNEL_SCENE, payload)
  trackEvent('error_scene', {
    message: message,
    payload: finalPayload
  })
  ReportRollbarError(new Error(message), finalPayload)
}

function GetErrorPayload(context: ErrorContextTypes, additionalPayload: any) {
  const result = {
    context: context,
    ...additionalPayload
  }
  return result
}

function ReportRollbarError(error: Error, payload: any) {
  if (window.Rollbar) {
    window.Rollbar.critical(error, payload)
  }
}
