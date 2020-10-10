import React from "react";

import { Meta, Story } from "@storybook/react";
import { EthLogin, EthLoginProps } from "./EthLogin";

export default {
  title: "Explorer/auth/EthLogin",
  args: {
    terms: true,
    loading: false,
    onLogin: () => {},
    onTermChange: () => {},
  } as EthLoginProps,
  component: EthLogin,
} as Meta;

export const Template: Story<EthLoginProps> = (args: EthLoginProps) => (
  <EthLogin {...args} />
);
