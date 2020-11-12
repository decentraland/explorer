import { action } from 'typesafe-actions'
import {
  COMMS_COULD_NOT_BE_ESTABLISHED,
  errorMessage,
  ExecutionLifecycleEvent,
  MOBILE_NOT_SUPPORTED,
  NETWORK_MISMATCH,
  NEW_LOGIN,
  NO_WEBGL_COULD_BE_CREATED,
  NOT_INVITED
} from './types'
import { StoreContainer } from 'shared/store/rootTypes'
import Html from '../Html'

declare const globalThis: StoreContainer

export let aborted = false

export function bringDownClientAndShowError(event: ExecutionLifecycleEvent) {
  if (aborted) {
    return
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
      : 'fatal'
  globalThis.globalStore && globalThis.globalStore.dispatch(errorMessage(targetError))
  Html.showErrorModal(targetError)
  aborted = true
}

export function ReportFatalError(event: ExecutionLifecycleEvent) {
  bringDownClientAndShowError(event)
  globalThis.globalStore && globalThis.globalStore.dispatch(action(event))
}
