import React, { useState } from "react";
import { WalletSelector } from "./wallet/WalletSelector";
import { Logo } from "../common/Logo";
import "./EthLogin.css";

export interface EthLoginProps {
  onLogin: (provider: string) => void;
  onGuest: () => void;
}

export const EthLogin: React.FC<EthLoginProps> = (props) => {
  const [wallet, setWallet] = useState(false);
  return (
    <div className="">
      <div className="eth-login-description">
        <Logo />
        <p>
          Increase yourself into the first virtual world fully owned by its
          users.
        </p>
      </div>
      <div id="eth-login-confirmation-wrapper">
        <button
          className="eth-login-confirm-button"
          onClick={() => setWallet(true)}
        >
          Sign In - Up
        </button>
        <br />
        <button className="eth-login-guest-button" onClick={props.onGuest}>
          Play as Guest
        </button>
      </div>
      <WalletSelector
        show={wallet}
        onClick={props.onLogin}
        onCancel={() => setWallet(false)}
      />
    </div>
  );
};
