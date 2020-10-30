import React, { useState } from "react";
import { WalletSelector } from "./wallet/WalletSelector";
import { LoginHeader } from "./LoginHeader";
import "./EthLogin.css";

export interface EthLoginProps {
  onLogin: (provider: string) => void;
  onGuest: () => void;
}

export const EthLogin: React.FC<EthLoginProps> = (props) => {
  const [wallet, setWallet] = useState(false);
  return (
    <div className="">
      <LoginHeader />
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
