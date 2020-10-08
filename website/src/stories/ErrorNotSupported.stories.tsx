import React from "react";
import { Meta, Story } from "@storybook/react";
import { ErrorNotSupported } from "../components/errors/ErrorNotSupported";

export default {
  title: "Explorer/Errors/ErrorNotSupported",
  component: ErrorNotSupported,
} as Meta;

export const Template: Story = () => <ErrorNotSupported />;
