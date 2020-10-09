import React from "react";
import { Meta, Story } from "@storybook/react";
import { ErrorNotSupported } from "./ErrorNotSupported";

export default {
  title: "Explorer/Errors/ErrorNotSupported",
  component: ErrorNotSupported,
} as Meta;

export const Template: Story = () => <ErrorNotSupported />;
