import React from "react";
import { Meta, Story } from "@storybook/react";
// import { action } from "@storybook/addon-actions";
import { NetworkWarning, NetworkWarningProps } from "./NetworkWarning";

export default {
  title: "Explorer/Warnings/NetworkWarning",
  args: {},
  component: NetworkWarning,
  argTypes: { onClose: { action: "closed clicked" } },
} as Meta;

export const Template: Story<NetworkWarningProps> = (args) => (
  <NetworkWarning {...args} />
);
