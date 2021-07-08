import React from "react"
import { connect } from "react-redux"
import Overlay from "./common/Overlay"
import ErrorContainer from "./errors/ErrorContainer"
import LoginContainer from "./auth/LoginContainer"
import { Audio } from "./common/Audio"
import { StoreType } from "../state/redux"
import "./App.css"

function mapStateToProps(state: StoreType): AppProps {
  return {
    error: !!state.error,
    sound: true, // TODO: sound must be true after the first click
  }
}

export interface AppProps {
  error: boolean
  sound: boolean
}

const App: React.FC<AppProps> = (props) => (
  <div>
    {props.sound && <Audio track="/tone4.mp3" play={true} />}
    <Overlay />
    {!props.error && <LoginContainer />}
    {props.error && <ErrorContainer />}
  </div>
)

export default connect(mapStateToProps)(App)
