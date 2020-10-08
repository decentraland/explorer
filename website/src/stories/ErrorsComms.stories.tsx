import React from "react";
import { Meta, Story } from "@storybook/react";
import { ErrorComms } from "../components/errors/ErrorComms";

export default {
  title: "Explorer/Errors/ErrorComms",
  component: ErrorComms,
} as Meta;

export const Template: Story = () => <ErrorComms />;
