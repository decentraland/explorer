import React from "react";
import { Navbar } from "../common/Navbar";
import { Footer } from "../common/Footer";
import { EthLogin } from "./EthLogin";
import { EthConnectAdvice } from "./EthConnectAdvice";
import { EthSignAdvice } from "./EthSignAdvice";
import { Logo } from "../common/Logo";
import { connect } from "react-redux";
import SignUpContainer from "./SignUpContainer";

export enum LoginStage {
  LOADING = "loading",
  SING_IN = "signIn",
  SING_UP = "signUp",
  CONNECT_ADVICE = "connect_advice",
  SING_ADVICE = "sign_advice",
  COMPLETED = "completed",
}

const mapStateToProps = (state: any) => ({
  terms: !!state.session.tos,
  stage: state.session.loginStage,
});

const mapDispatchToProps = (dispatch: any) => ({
  onLogin: (provider: string) => {
    console.log("[Authenticate]");
    return dispatch({ type: "[Authenticate]", payload: { provider } });
    // return dispatch({ type: "[Request] Login", payload: { provider } });
  },
  onGuest: () =>
    dispatch({ type: "[Request] Login", payload: { provider: "Guest" } }),
  onTermsChange: (e: React.ChangeEvent<HTMLInputElement>) =>
    dispatch({ type: "UPDATE_TOS", payload: e.target.checked }),
});

export interface LoginContainerProps {
  terms: boolean;
  stage: LoginStage;
  onLogin: (provider: string) => void;
  onGuest: () => void;
  onTermsChange: (e: React.ChangeEvent<HTMLInputElement>) => void;
}

export const LoginContainer: React.FC<LoginContainerProps> = (props) => (
  <React.Fragment>
    {props.stage !== LoginStage.COMPLETED && (
      <div className="login">
        <Navbar />
        <div className="eth-login-popup">
          <Logo />
          {(props.stage === LoginStage.SING_IN ||
            props.stage === LoginStage.LOADING) && (
            <EthLogin
              terms={props.terms}
              loading={props.stage === LoginStage.LOADING}
              onLogin={props.onLogin}
              onGuest={props.onGuest}
              onTermChange={props.onTermsChange}
            />
          )}
          {props.stage === LoginStage.CONNECT_ADVICE && (
            <EthConnectAdvice onLogin={props.onLogin} />
          )}
          {props.stage === LoginStage.SING_ADVICE && <EthSignAdvice />}
          {props.stage === LoginStage.SING_UP && <SignUpContainer />}
        </div>
        <Footer />
      </div>
    )}
  </React.Fragment>
);
export default connect(mapStateToProps, mapDispatchToProps)(LoginContainer);
