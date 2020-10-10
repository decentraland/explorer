import React from "react";
import { connect } from "react-redux";
import Overlay from "./Overlay";
import ErrorContainer from "./ErrorContainer";
import LoginContainer from "./LoginContainer";
import LoadingContainer from "./LoadingContainer";
import { Audio } from "./Audio";
import WarningContainer from "./WarningContainer";

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
