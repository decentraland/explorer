import React from "react";

import { Meta, Story } from "@storybook/react";
import { TermsOfServices, TermsOfServicesProps } from "./TermsOfServices";

export default {
  title: "Explorer/auth/TermsOfServices",
  args: {},
  component: TermsOfServices,
  argTypes: {
    handleCancel: { action: "canceling..." },
    handleAgree: { action: "agree..." },
  },
} as Meta;

export const Template: Story<TermsOfServicesProps> = (args) => (
  <TermsOfServices {...args} />
);
