import React from "react";
import { connect } from "react-redux";
import Overlay from "./common/Overlay";
import ErrorContainer from "./errors/ErrorContainer";
import LoginContainer from "./auth/LoginContainer";
import LoadingContainer from "./loading/LoadingContainer";
import { Audio } from "./common/Audio";
import WarningContainer from "./warning/WarningContainer";
import "./App.css";
const mapStateToProps = (state: any) => {
  return {
    error: !!state.loading.error,
  };
};

export interface AppProps {
  error: boolean;
}

const App: React.FC<AppProps> = (props) => (
  <div>
    <Audio track="/tone4.mp3" play={true} />
    <Overlay />
    <WarningContainer />
    {!props.error && <LoadingContainer />}
    {!props.error && <LoginContainer />}
    {props.error && <ErrorContainer />}
  </div>
);

export default connect(mapStateToProps)(App);
