import React from "react";
import { Meta, Story } from "@storybook/react";
import { WalletSelector, WalletSelectorProps } from "./WalletSelector";

export default {
  title: "Explorer/auth/WalletSelector",
  args: {
    show: true,
  },
  component: WalletSelector,
  argTypes: {
    onClick: { action: "clicked" },
    onCancel: { action: "Canceled" },
  },
} as Meta;

export const Template: Story<WalletSelectorProps> = (args) => (
  <WalletSelector {...args} />
);
