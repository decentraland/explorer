import React from "react";
import { Meta, Story } from "@storybook/react";
import { WalletButton, WalletButtonProps } from "./WalletButton";

export default {
  title: "Explorer/auth/WalletButton",
  args: {
    logo: "",
    title: "",
    description: "",
  },
  component: WalletButton,
  argTypes: {
    onClick: { action: "clicked" },
  },
} as Meta;

const Template: Story<WalletButtonProps> = (args) => <WalletButton {...args} />;

export const MetamaskButton = Template.bind({});
MetamaskButton.args = {
  ...Template.args,
  logo: "Metamask",
};

export const FortmaticButton = Template.bind({});
FortmaticButton.args = {
  ...Template.args,
  logo: "Fortmatic",
};
