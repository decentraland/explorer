import React from "react";
import { Meta, Story } from "@storybook/react";
import { EthConnectAdvice, EthConnectAdviceProps } from "./EthConnectAdvice";

export default {
  title: "Explorer/login/EthConnectAdvice",
  args: {
    onLogin: () => {},
  } as EthConnectAdviceProps,
  component: EthConnectAdvice,
} as Meta;

export const Template: Story<EthConnectAdviceProps> = (args) => (
  <EthConnectAdvice {...args} />
);
