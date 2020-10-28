import React from "react";
import { connect } from "react-redux";
import { Passport } from "./Passport";
import { TermsOfServices } from "./TermsOfServices";

export interface SignUpContainerProps {
  name: string;
  email: string;
  stage: string;
  handleForm: any;
  handleAgree: any;
  handleCancel: any;
  handleEditAvatar: any;
}

export const SignUpContainer: React.FC<SignUpContainerProps> = (props) => (
  <React.Fragment>
    {props.stage === "passport" && (
      <Passport
        name={props.name}
        email={props.email}
        onSubmit={props.handleForm}
        onEditAvatar={props.handleEditAvatar}
      />
    )}
    {props.stage === "terms" && (
      <TermsOfServices
        handleCancel={props.handleCancel}
        handleAgree={props.handleAgree}
      />
    )}
  </React.Fragment>
);

const mapStateToProps = (state: any) => ({
  name: state.session.signup.profile?.name,
  email: state.session.signup.profile?.email,
  stage: state.session.signup.stage,
});

const mapDispatchToProps = (dispatch: any) => ({
  handleForm: (name: string, email: string) => {
    console.log("[SIGNUP_FORM]");
    dispatch({ type: "[SIGNUP_FORM]", payload: { name, email } });
    dispatch({ type: "[SIGNUP_STAGE]", payload: { stage: "terms" } });
  },
  handleAgree: () => dispatch({ type: "[SIGNUP]" }),
  handleCancel: () => {
    console.log("[SIGNUP_STAGE]- back to passport");
    dispatch({ type: "[SIGNUP_STAGE]", payload: { stage: "passport" } });
  },
  handleEditAvatar: () => {
    console.log("[SIGNUP_STAGE]- back to avatar editor");
    // dispatch({ type: "[SIGNUP_STAGE]", payload: { stage: "editor" } });
  },
});

export default connect(mapStateToProps, mapDispatchToProps)(SignUpContainer);
