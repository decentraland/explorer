import React from "react";
import { Modal } from "../../common/Modal";
import { WalletButton } from "./WalletButton";
import { Spinner } from "../../common/Spinner";

import MetamaskLogo from "../../../images/metamask.svg";
import FortmaticLogo from "../../../images/fortmatic.svg";
import "./WalletSelector.css";

export interface WalletSelectorProps {
  show: boolean;
  loading: boolean;
  metamask: boolean;
  onClick: (provider: string) => void;
  onCancel: () => void;
}

export const WalletSelector: React.FC<WalletSelectorProps> = ({
  show,
  metamask,
  loading,
  onClick,
  onCancel,
}) => {
  return show ? (
    <Modal handleClose={onCancel}>
      <div className="walletSelector">
        <h2 className="walletSelectorTitle">Sign In - Up</h2>
        <div className="walletButtonContainer">
          {loading && <Spinner />}
          {!loading && (
            <WalletButton
              title="Metamask"
              logo={MetamaskLogo}
              disabled={!metamask}
              description="Using a browser extension"
              onClick={() => onClick("Metamask")}
            />
          )}
          {!loading && (
            <WalletButton
              title="Fortmatic"
              logo={FortmaticLogo}
              description="Using your email account"
              onClick={() => onClick("Fortmatic")}
            />
          )}
        </div>
      </div>
    </Modal>
  ) : null;
};
