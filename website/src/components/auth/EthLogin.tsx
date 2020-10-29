import React, { useState } from "react";
import { Spinner } from "../common/Spinner";
import { WalletSelector } from "./WalletSelector";
import pngLogo from "../../images/logo-dcl.png";
import "./EthLogin.css";

export interface EthLoginProps {
  terms: boolean;
  loading: boolean;
  onLogin: (provider: string) => void;
  onGuest: () => void;
  onTermChange: any;
}

export const EthLogin: React.FC<EthLoginProps> = (props) => {
  const [wallet, setWallet] = useState(false);
  return (
    <div className="loginContainer">
      <div className="eth-login-description">
        <img alt="Decentraland" className="eth-login-logo" src={pngLogo} />
        <p>
          Increase yourself into the first virtual world fully owned by its
          users.
        </p>
      </div>
      {props.loading ? (
        <Spinner />
      ) : (
        <div id="eth-login-confirmation-wrapper">
          <button
            className="eth-login-confirm-button"
            disabled={!props.terms}
            onClick={() => setWallet(true)}
          >
            Sign In - Up
          </button>
          <br />
          <button
            className="eth-login-guest-button"
            disabled={!props.terms}
            onClick={props.onGuest}
          >
            Play as Guest
          </button>
        </div>
      )}
      <WalletSelector
        show={wallet}
        onClick={props.onLogin}
        onCancel={() => setWallet(false)}
      />
    </div>
  );
};
