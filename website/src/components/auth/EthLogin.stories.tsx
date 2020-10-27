import React from "react";

import { Meta, Story } from "@storybook/react";
import { EthLogin, EthLoginProps } from "./EthLogin";

export default {
  title: "Explorer/auth/EthLogin",
  args: {
    terms: true,
    loading: false,
  } as EthLoginProps,
  component: EthLogin,
  argTypes: {
    onLogin: { action: "singing in/up..." },
    onGuest: { action: "guest click..." },
    onTermsChange: { action: "terms changed..." },
  },
} as Meta;

export const Template: Story<EthLoginProps> = (args: EthLoginProps) => (
  <EthLogin {...args} />
);
