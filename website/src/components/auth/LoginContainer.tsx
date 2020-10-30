import React from "react";
import { Navbar } from "../common/Navbar";
import { Footer } from "../common/Footer";
import { EthLogin } from "./EthLogin";
import { EthConnectAdvice } from "./EthConnectAdvice";
import { EthSignAdvice } from "./EthSignAdvice";
import { InitialLoading } from "./InitialLoading";
import { connect } from "react-redux";
import SignUpContainer from "./SignUpContainer";
import "./LoginContainer.css";

export enum LoginStage {
  LOADING = "loading",
  SIGN_IN = "signIn",
  SIGN_UP = "signUp",
  CONNECT_ADVICE = "connect_advice",
  SIGN_ADVICE = "sign_advice",
  COMPLETED = "completed",
}

const mapStateToProps = (state: any) => ({
  stage: state.session.loginStage,
  subStage: state.session.signup.stage,
});

const mapDispatchToProps = (dispatch: any) => ({
  onLogin: (provider: string) =>
    dispatch({ type: "[Authenticate]", payload: { provider } }),
  onGuest: () =>
    dispatch({ type: "[Request] Login", payload: { provider: "Guest" } }),
});

export interface LoginContainerProps {
  stage: LoginStage;
  subStage: string;
  onLogin: (provider: string) => void;
  onGuest: () => void;
}

export const LoginContainer: React.FC<LoginContainerProps> = (props) => {
  const shouldShow =
    LoginStage.COMPLETED !== props.stage && props.subStage !== "avatar";
  return (
    <React.Fragment>
      {shouldShow && (
        <div className="LoginContainer">
          <Navbar />
          <div className="eth-login-popup">
            {props.stage === LoginStage.LOADING && <InitialLoading />}
            {props.stage === LoginStage.SIGN_IN && (
              <EthLogin onLogin={props.onLogin} onGuest={props.onGuest} />
            )}
            {props.stage === LoginStage.CONNECT_ADVICE && (
              <EthConnectAdvice onLogin={props.onLogin} />
            )}
            {props.stage === LoginStage.SIGN_ADVICE && <EthSignAdvice />}
            {props.stage === LoginStage.SIGN_UP && <SignUpContainer />}
          </div>
          <Footer />
        </div>
      )}
    </React.Fragment>
  );
};
export default connect(mapStateToProps, mapDispatchToProps)(LoginContainer);
