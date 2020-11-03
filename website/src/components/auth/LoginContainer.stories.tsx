import React from "react";
import { Meta, Story } from "@storybook/react";
import {
  LoginContainer,
  LoginContainerProps,
  LoginStage,
} from "./LoginContainer";

export default {
  title: "Explorer/Login",
  args: {
    stage: LoginStage.LOADING,
    subStage: "",
  } as LoginContainerProps,
  component: LoginContainer,
  argTypes: {
    onLogin: { action: "signing in..." },
    onTermsChange: { action: "terms changed..." },
  },
} as Meta;

const Template: Story<LoginContainerProps> = (args) => (
  <LoginContainer {...args} />
);

export const LoadingState = Template.bind({});
LoadingState.args = {
  ...Template.args,
};

export const signInUp = Template.bind({});
signInUp.args = {
  ...Template.args,
  stage: LoginStage.SIGN_IN,
};

export const ConnectAdvice = Template.bind({});
ConnectAdvice.args = {
  ...Template.args,
  stage: LoginStage.CONNECT_ADVICE,
};

export const SignAdvice = Template.bind({});
SignAdvice.args = {
  ...Template.args,
  stage: LoginStage.SIGN_ADVICE,
};
