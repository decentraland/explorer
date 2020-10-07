import * as React from 'react'
import LoginContainer from './LoginContainer'
import LoadingContainer from './LoadingContainer'
import Overlay from './Overlay'
import { Audio } from './Audio'

export const App: React.FC = (props) => (
  <div>
    <Audio track="/tone4.mp3" play={true} />
    <Overlay />
    <LoadingContainer />
    <LoginContainer />
    {/*<ErrorContainer />*/}
  </div>
)
