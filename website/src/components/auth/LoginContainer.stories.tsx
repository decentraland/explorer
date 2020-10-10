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
    terms: false,
    stage: LoginStage.LOADING,
  } as LoginContainerProps,
  component: LoginContainer,
  argTypes: {
    onLogin: { action: "singing in..." },
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

export const signInDisabled = Template.bind({});
signInDisabled.args = {
  ...Template.args,
  stage: LoginStage.SING_IN,
};

export const signInEnable = Template.bind({});
signInEnable.args = {
  ...Template.args,
  terms: true,
  stage: LoginStage.SING_IN,
};

export const ConnectAdvice = Template.bind({});
ConnectAdvice.args = {
  ...Template.args,
  stage: LoginStage.CONNECT_ADVICE,
};

export const SignAdvice = Template.bind({});
SignAdvice.args = {
  ...Template.args,
  stage: LoginStage.SING_ADVICE,
};
