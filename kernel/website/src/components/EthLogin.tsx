import * as React from 'react'
import './EthLogin.css'
import { Spinner } from './Spinner'
import { TermOfService } from './TermOfService'

export interface EthLoginProps {
  terms: boolean
  loading: boolean
  onLogin: any
  onTermChange: any
}

export const EthLogin: React.FC<EthLoginProps> = (props) => {
  return (
    <React.Fragment>
      <div className="eth-login-description">Enter the first virtual world fully owned by its users.</div>
      {props.loading ? (
        <Spinner />
      ) : (
        <div id="eth-login-confirmation-wrapper">
          <TermOfService checked={props.terms} onChange={props.onTermChange} />
          <button
            id="eth-login-confirm-button"
            className="eth-login-confirm-button1"
            disabled={!props.terms}
            onClick={props.onLogin}
          >
            Start Exploring
          </button>
        </div>
      )}
    </React.Fragment>
  )
}
