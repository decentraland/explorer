import React, { useState } from "react";
import { WalletSelector } from "./wallet/WalletSelector";
import { LoginHeader } from "./LoginHeader";
import { Spinner } from "../common/Spinner";
import { Avatars } from "../common/Avatars";
import "./EthLogin.css";

export interface EthLoginProps {
  loading: boolean;
  provider: string | null | undefined;
  showWallet?: boolean;
  hasWallet?: boolean;
  hasMetamask?: boolean;
  onLogin: (provider: string) => void;
  onGuest: () => void;
}

export const EthLogin: React.FC<EthLoginProps> = (props) => {
  const [showWallet, setShowWallet] = useState(props.showWallet || false);
  const walletLoading = props.loading && showWallet;
  const isLoading = props.loading || showWallet

  function handlePlay(event: React.MouseEvent) {
    if (props.provider) {
      return props.onLogin(props.provider);
    }

    setShowWallet(true);
  }

  function handlePlayAsGuest(event: React.MouseEvent) {
    if (props.onGuest) {
      props.onGuest()
    }
  }

  return (
    <div className="eth-login">
      <LoginHeader />
      <Avatars />
      <div id="eth-login-confirmation-wrapper">
        {isLoading && <Spinner />}
        {!isLoading && (
          <button className="eth-login-confirm-button" onClick={handlePlay}>
            Play
          </button>
        )}
        {!isLoading && !props.hasWallet && (
          <button className="eth-login-guest-button" onClick={handlePlayAsGuest}>
            Enter as Guest
          </button>
        )}
      </div>
      <WalletSelector
        show={showWallet}
        metamask={!!props.hasMetamask}
        loading={walletLoading}
        onClick={props.onLogin}
        onCancel={() => setShowWallet(false)}
      />
    </div>
  );
};
