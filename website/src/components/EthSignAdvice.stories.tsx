import React from "react";

import { Meta, Story } from "@storybook/react";
import { EthSignAdvice } from "./EthSignAdvice";

export default {
  title: "Explorer/login/EthSignAdvice",
  component: EthSignAdvice,
} as Meta;

export const Template: Story = () => <EthSignAdvice />;
