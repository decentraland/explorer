import * as React from 'react'
import { connect } from 'react-redux'
import LoginContainer from './LoginContainer'
import LoadingContainer from './LoadingContainer'
import Overlay from './Overlay'
import { Audio } from './Audio'
import ErrorContainer from './ErrorContainer'

const mapStateToProps = (state: any) => {
  return {
    error: !!state.loading.error
  }
}

const App: React.FC = (props) => (
  <div>
    <Audio track="/tone4.mp3" play={true} />
    {!props.error && <Overlay />}
    {!props.error && <LoadingContainer />}
    {!props.error && <LoginContainer />}
    {props.error && <ErrorContainer />}
  </div>
)

export default connect(mapStateToProps)(App)
