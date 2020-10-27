import React, { useState } from "react";
import "./EthLogin.css";
import { Spinner } from "../common/Spinner";
import { WalletSelector } from "./WalletSelector";
import { TermOfServices } from "./TermsOfServices";
import { Passport } from "./Passport";

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
    <React.Fragment>
      <div className="eth-login-description">
        Increase yourself into the first virtual world fully owned by its users.
      </div>
      {props.loading ? (
        <Spinner />
      ) : (
        <div id="eth-login-confirmation-wrapper">
          <button
            id="eth-login-confirm-button"
            className="eth-login-confirm-button1"
            disabled={!props.terms}
            onClick={() => setWallet(true)}
          >
            Sign In - Up
          </button>
          <br />
          <button
            className="eth-login-confirm-button1"
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
      <TermOfServices show={false} />
      <Passport show={false} />
    </React.Fragment>
  );
};
