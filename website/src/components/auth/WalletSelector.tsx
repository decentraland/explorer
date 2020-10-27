import React from "react";
import { Modal } from "../common/Modal";

export interface WalletSelectorProps {
  show: boolean;
  onClick: (provider: string) => void;
  onCancel: () => void;
}

export const WalletSelector: React.FC<WalletSelectorProps> = ({
  show,
  onClick,
  onCancel,
}) => {
  return show ? (
    <Modal handleClose={onCancel}>
      <div className="column">
        <h2>Sign In - Up</h2>
      </div>
      <div
        className="column"
        style={{ flex: "0 0 100%", padding: "0 100px 30px" }}
      >
        <button
          className="button full primary"
          onClick={() => onClick("Metamask")}
        >
          <img alt="" src="/images/metamask.svg" width="26" height="25" />
          <span style={{ textTransform: "none" }}>Metamask</span>
        </button>
        <button
          className="button full primary"
          onClick={() => onClick("Fortmatic")}
        >
          <img alt="" src="/images/fortmatic.svg" width="26" height="25" />
          <span style={{ textTransform: "none" }}>Fortmatic</span>
        </button>
      </div>
    </Modal>
  ) : null;
};
