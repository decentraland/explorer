import React from 'react'
import { connect } from 'react-redux'
import ErrorContainer from './errors/ErrorContainer'
import LoginContainer from './auth/LoginContainer'
import { Audio } from './common/Audio'
import WarningContainer from './warning/WarningContainer'
import './App.css'
const mapStateToProps = (state: any) => {
  return {
    error: !!state.loading.error,
    sound: state.loading.showLoadingScreen
  }
}

export interface AppProps {
  error: boolean
  sound: boolean
}

const App: React.FC<AppProps> = (props) => (
  <div>
    {props.sound && <Audio track="/tone4.mp3" play={true} />}
    <WarningContainer />
    {!props.error && <LoginContainer />}
    {props.error && <ErrorContainer />}
  </div>
)

export default connect(mapStateToProps)(App)
