import React from "react";
import { Meta, Story } from "@storybook/react";
import {
  WarningContainer,
  WarningContainerProps,
  WARNINGS,
} from "./WarningContainer";

export default {
  title: "Explorer/Warnings/Container",
  args: {
    type: WARNINGS.NETWORK_WARNING,
  },
  component: WarningContainer,
  argTypes: { onClose: { action: "closed clicked" } },
} as Meta;

export const Template: Story<WarningContainerProps> = (args) => (
  <WarningContainer {...args} />
);
