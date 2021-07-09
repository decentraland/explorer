import React from 'react'
import { connect } from 'react-redux'
import { connection } from 'decentraland-connect/dist/index'
import { ProviderType } from 'decentraland-connect/dist/types'
import { Navbar } from '../common/Navbar'
import { EthLogin } from './EthLogin'
import { EthConnectAdvice } from './EthConnectAdvice'
import { EthSignAdvice } from './EthSignAdvice'
import { Container } from '../common/Container'
import { BeginnersGuide } from './BeginnersGuide'
import { BigFooter } from '../common/BigFooter'
import './LoginContainer.css'
import { StoreType } from '../../state/redux'

export enum LoginStage {
  LOADING = 'loading',
  SIGN_IN = 'signIn',
  SIGN_UP = 'signUp',
  CONNECT_ADVICE = 'connect_advice',
  SIGN_ADVICE = 'sign_advice',
  COMPLETED = 'completed'
}

const mapStateToProps = (state: StoreType): LoginContainerProps => {
  // test all connectors
  const enableProviders = new Set([
    ProviderType.INJECTED, // Ready
    ProviderType.FORTMATIC // Ready
    // ProviderType.WALLET_CONNECT, // Missing configuration
  ])
  const availableProviders = connection.getAvailableProviders().filter((provider) => enableProviders.has(provider))
  return {
    stage: state.session.kernelState?.loginStatus,
    signing: state.session.kernelState?.signing || false,
    availableProviders,
    engineReady: state.kernel.ready
  }
}

const mapDispatchToProps = (dispatch: any) => ({
  onLogin: (provider: ProviderType | null) => dispatch({ type: '[Authenticate]', payload: { provider } })
})

export interface LoginContainerProps {
  stage?: LoginStage
  signing: boolean
  availableProviders: ProviderType[]
  engineReady: boolean
}

export interface LoginContainerDispathc {
  onLogin: (provider: ProviderType | null) => void
}

export const LoginContainer: React.FC<LoginContainerProps & LoginContainerDispathc> = (props) => {
  const loading = props.stage === LoginStage.LOADING || !props.engineReady
  const full = loading || props.stage === LoginStage.SIGN_IN
  const shouldShow = LoginStage.COMPLETED !== props.stage && LoginStage.SIGN_UP !== props.stage
  return (
    <React.Fragment>
      {shouldShow && (
        <div className={'LoginContainer' + (full ? ' FullPage' : '')}>
          {/* Nabvar */}
          <Navbar full={full} />
          <main>
            <Container className="eth-login-popup">
              {full && (
                <EthLogin
                  availableProviders={props.availableProviders}
                  onLogin={props.onLogin}
                  signing={props.signing}
                />
              )}
              {props.stage === LoginStage.CONNECT_ADVICE && <EthConnectAdvice onLogin={props.onLogin} />}
              {props.stage === LoginStage.SIGN_ADVICE && <EthSignAdvice />}
            </Container>
          </main>
          {full && <BeginnersGuide />}
          {full && <BigFooter />}
        </div>
      )}
    </React.Fragment>
  )
}
export default connect(mapStateToProps, mapDispatchToProps)(LoginContainer)
