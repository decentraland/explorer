import React from "react";
import { Navbar } from "./Navbar";
import { Footer } from "./Footer";
import { EthLogin } from "./EthLogin";
import { EthConnectAdvice } from "./EthConnectAdvice";
import { EthSignAdvice } from "./EthSignAdvice";
import { Logo } from "./Logo";
import { connect } from "react-redux";

export enum LoginStage {
  LOADING = "loading",
  SING_IN = "signIn",
  CONNECT_ADVICE = "connect_advice",
  SING_ADVICE = "sign_advice",
  COMPLETED = "completed",
}

const mapStateToProps = (state: any) => ({
  terms: !!state.session.tos,
  stage: state.session.loginStage,
});

const mapDispatchToProps = (dispatch: any) => ({
  onLogin: () =>
    dispatch({ type: "[Request] Login", payload: { provider: "Metamask" } }),
  onTermsChange: (e: React.ChangeEvent<HTMLInputElement>) =>
    dispatch({ type: "UPDATE_TOS", payload: e.target.checked }),
});

export interface LoginContainerProps {
  terms: boolean;
  stage: LoginStage;
  onLogin: any;
  onTermsChange: any;
}

const LoginContainer: React.FC<LoginContainerProps> = (props) => (
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
              onTermChange={props.onTermsChange}
            />
          )}
          {props.stage === LoginStage.CONNECT_ADVICE && (
            <EthConnectAdvice onLogin={props.onLogin} />
          )}
          {props.stage === LoginStage.SING_ADVICE && <EthSignAdvice />}
        </div>
        <Footer />
      </div>
    )}
  </React.Fragment>
);
export default connect(mapStateToProps, mapDispatchToProps)(LoginContainer);
