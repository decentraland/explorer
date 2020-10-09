import React from "react";
import { Meta, Story } from "@storybook/react";
import { ErrorFatal } from "./ErrorFatal";

export default {
  title: "Explorer/Errors/ErrorFatal",
  component: ErrorFatal,
} as Meta;

export const Template: Story = () => <ErrorFatal />;
