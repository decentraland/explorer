import React from "react";
import { Meta, Story } from "@storybook/react";
import { ErrorNewLogin } from "../components/errors/ErrorNewLogin";

export default {
  title: "Explorer/Errors/ErrorNewLogin",
  component: ErrorNewLogin,
} as Meta;

export const Template: Story = () => <ErrorNewLogin />;
