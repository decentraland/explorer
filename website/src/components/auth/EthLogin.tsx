import React, { useState, useEffect } from "react";
import { WalletSelector } from "./wallet/WalletSelector";
import { LoginHeader } from "./LoginHeader";
import { Spinner } from "../common/Spinner";
import { Avatars } from "../common/Avatars";
import "./EthLogin.css";

export interface EthLoginProps {
  loading: boolean;
  provider: string | null | undefined;
  showWalletSelector?: boolean;
  showGuestLogin?: boolean;
  onLogin: (provider: string) => void;
  onGuest: () => void;
}

export const EthLogin: React.FC<EthLoginProps> = (props) => {
  const [showWalletSelector, setShowWalletSelector] = useState(props.showWalletSelector || false);
  const [showGuestLogin, setShowGuestLogin] = useState(props.showGuestLogin || false);
  useEffect(() => {
    if (!props.showGuestLogin && !(window as any).ethereum) {
      setShowGuestLogin(true)
    }
  }, [props.showGuestLogin])

  const walletLoading = props.loading && showWalletSelector;
  const isLoading = props.loading || showWalletSelector

  function handlePlay(event: React.MouseEvent) {
    if (props.provider) {
      return props.onLogin(props.provider);
    }

    setShowWalletSelector(true);
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
        {!isLoading && showGuestLogin && (
          <button className="eth-login-guest-button" onClick={handlePlayAsGuest}>
            Enter as Guest
          </button>
        )}
      </div>
      <WalletSelector
        show={showWalletSelector}
        loading={walletLoading}
        onClick={props.onLogin}
        onCancel={() => setShowWalletSelector(false)}
      />
    </div>
  );
};
