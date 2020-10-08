import React from "react";

import { Meta, Story } from "@storybook/react";
import { Logo } from "../components/Logo";

export default {
  title: "Explorer/base/Logo",
  component: Logo,
} as Meta;

export const Template: Story = () => <Logo />;
