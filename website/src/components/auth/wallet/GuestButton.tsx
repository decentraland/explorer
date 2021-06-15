import React from "react";
import "./GuestButton.css";

export interface WalletButtonProps {
  active?: boolean;
  onClick: (providerType: null) => void;
}

export const GuestButton: React.FC<WalletButtonProps> = ({
  active,
  onClick,
}) => {

  function handleClick(event: React.MouseEvent<HTMLAnchorElement>) {
    if (active !== false) {
      event.preventDefault();
      if (onClick) {
        onClick(null);
      }
    }
  }

  return (
    <a
      className={`guestButton ${active ? 'active' : 'inactive'}`}
      href="#"
      onClick={handleClick}
      target="_blank"
      rel="noopener noreferrer"
    >Enter as Guest</a>
  );
};
