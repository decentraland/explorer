import React from "react";
import { Meta, Story } from "@storybook/react";
import { ProgressBar, ProgressBarProps } from "../components/ProgressBar";

export default {
  title: "Explorer/base/ProgressBar",
  args: {
    percentage: 10,
  },
  component: ProgressBar,
} as Meta;

export const Template: Story<ProgressBarProps> = (args) => (
  <ProgressBar {...args} />
);
