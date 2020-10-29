import React from "react";
import { Meta, Story } from "@storybook/react";
import { Passport, PassportProps } from "./Passport";

export default {
  title: "Explorer/auth/Passport",
  args: {
    name: "",
    email: "",
  },
  component: Passport,
  argTypes: {
    onSubmit: { action: "submit..." },
    onEditAvatar: { action: "Go to Avatar Editor..." },
  },
} as Meta;

export const Template: Story<PassportProps> = (args) => <Passport {...args} />;
