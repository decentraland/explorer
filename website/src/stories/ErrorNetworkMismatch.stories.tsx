import React from "react";
import { Meta, Story } from "@storybook/react";
import {
  ErrorNetworkMismatch,
  ErrorNetworkMismatchProps,
} from "../components/errors/ErrorNetworkMismatch";

export default {
  title: "Explorer/Errors/ErrorNetworkMismatch",
  args: {
    details: {
      tld: "",
      tldNet: "",
      web3Net: "",
    },
  } as ErrorNetworkMismatchProps,
  component: ErrorNetworkMismatch,
} as Meta;

export const Template: Story<ErrorNetworkMismatchProps> = (args) => (
  <ErrorNetworkMismatch {...args} />
);
