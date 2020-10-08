import React from "react";

import { Meta, Story } from "@storybook/react";
import { TermOfService, TermOfServiceProps } from "../components/TermOfService";

export default {
  title: "Explorer/login/TermOfService",
  args: {
    checked: false,
  },
  component: TermOfService,
} as Meta;

export const Template: Story<TermOfServiceProps> = (args) => (
  <TermOfService {...args} />
);
