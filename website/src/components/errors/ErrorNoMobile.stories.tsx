import React from "react";
import { Meta, Story } from "@storybook/react";
import { ErrorNoMobile } from "./ErrorNoMobile";

export default {
  title: "Explorer/Errors/ErrorNoMobile",
  component: ErrorNoMobile,
} as Meta;

export const Template: Story = () => <ErrorNoMobile />;
