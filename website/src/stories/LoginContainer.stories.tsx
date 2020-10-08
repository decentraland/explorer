import React from "react";

import { Meta, Story } from "@storybook/react";
import LoginContainer, {
  LoginContainerProps,
} from "../components/LoginContainer";

export default {
  title: "Explorer/LoginContainer",
  args: {
    terms: true,
    stage: "loading",
  } as LoginContainerProps,
  component: LoginContainer,
} as Meta;

export const Template: Story = (args: any) => <LoginContainer {...args} />;
