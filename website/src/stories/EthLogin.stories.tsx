import React from "react";

import { Meta, Story } from "@storybook/react";
import { EthLogin, EthLoginProps } from "../components/EthLogin";

export default {
  title: "Explorer/login/EthLogin",
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
