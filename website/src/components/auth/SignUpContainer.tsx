import React from "react";
import { connect } from "react-redux";
import { Passport } from "./passport/Passport";
import { TermsOfServices } from "./terms/TermsOfServices";

export interface SignUpContainerProps {
  face: string;
  name: string;
  email: string;
  stage: string;
  handleForm: any;
  handleAgree: any;
  handleCancel: any;
  handleEditAvatar: any;
}

export const SignUpContainer: React.FC<SignUpContainerProps> = (props) => {
  return (
    <React.Fragment>
      {props.stage === "passport" && (
        <Passport
          face={props.face}
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
};

const mapStateToProps = (state: any) => {
  let face = state.session.signup.profile.avatar?.snapshots?.face256;
  if (face) {
    face = "data:image/jpg;base64," + face;
  }
  return {
    face,
    name: state.session.signup.profile?.name,
    email: state.session.signup.profile?.email,
    stage: state.session.signup.stage,
  };
};

const mapDispatchToProps = (dispatch: any) => ({
  handleForm: (name: string, email: string) => {
    dispatch({ type: "[SIGNUP_FORM]", payload: { name, email } });
    dispatch({ type: "[SIGNUP_STAGE]", payload: { stage: "terms" } });
  },
  handleAgree: () => dispatch({ type: "[SIGNUP]" }),
  handleCancel: () => {
    dispatch({ type: "[SIGNUP_STAGE]", payload: { stage: "passport" } });
  },
  handleEditAvatar: () => {
    // dispatch({ type: "[SIGNUP_STAGE]", payload: { stage: "editor" } });
  },
});

export default connect(mapStateToProps, mapDispatchToProps)(SignUpContainer);
