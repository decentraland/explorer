import * as React from 'react'
import { connect } from 'react-redux'
import { ErrorComms } from './errors/ErrorComms'
import { ErrorFatal } from './errors/ErrorFatal'
import { ErrorNoMobile } from './errors/ErrorNoMobile'
import { ErrorNewLogin } from './errors/ErrorNewLogin'
import { ErrorNetworkMismatch } from './errors/ErrorNetworkMismatch'
import { ErrorNotInvited } from './errors/ErrorNotInvited'
import { ErrorNotSupported } from './errors/ErrorNotSupported'

enum Error {
  FATAL = 'fatal',
  COMMS = 'comms',
  NEW_LOGIN = 'newlogin',
  NOT_MOBILE = 'nomobile',
  NOT_INVITED = 'notinvited',
  NOT_SUPPORTED = 'notsupported',
  NET_MISMATCH = 'networkmismatch'
}

const mapStateToProps = (state: any) => {
  return {
    error: state.loading.error || null,
    details: state.loading.tldError || null
  }
}

export interface ErrorContainerProps {
  error: string | null
  tldError: { tld: string; web3Net: string; tldNet: string } | null
}

const ErrorContainer: React.FC<ErrorContainerProps> = (props) => {
  return (
    <React.Fragment>
      {props.error === Error.FATAL && <ErrorFatal />}
      {props.error === Error.COMMS && <ErrorComms />}
      {props.error === Error.NEW_LOGIN && <ErrorNewLogin />}
      {props.error === Error.NOT_MOBILE && <ErrorNoMobile />}
      {props.error === Error.NOT_INVITED && <ErrorNotInvited />}
      {props.error === Error.NOT_SUPPORTED && <ErrorNotSupported />}
      {props.error === Error.NET_MISMATCH && <ErrorNetworkMismatch tld={props.tldError} />}
    </React.Fragment>
  )
}

export default connect(mapStateToProps)(ErrorContainer)
