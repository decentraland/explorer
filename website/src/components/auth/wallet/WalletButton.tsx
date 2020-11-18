import React, { useMemo } from "react";
import MetamaskLogo from "../../../images/login/metamask.svg";
import FortmaticLogo from "../../../images/login/fortmatic.svg";
import "./WalletButton.css";

export type WalletButtonLogo = 'Metamask' | 'Fortmatic'

export interface WalletButtonProps {
  logo: WalletButtonLogo
  onClick: (event: React.MouseEvent<HTMLDivElement>, logo: WalletButtonLogo) => void;
}

export const WalletButton: React.FC<WalletButtonProps> = ({
  logo,
  onClick,
}) => {

  const src = useMemo(() => {
    switch (logo) {
      case 'Fortmatic':
        return <img alt={logo} src={FortmaticLogo} className="fortmatic" />
      case 'Metamask':
      default:
        return <img alt={logo} src={MetamaskLogo} className="metamask" />
    }
  }, [logo])

  const title = useMemo(() => {
    switch (logo) {
      case 'Fortmatic':
        return 'Fortmatic'
      case 'Metamask':
      default:
        return 'Metamask'
    }
  }, [logo])

  const description = useMemo(() => {
    switch (logo) {
      case 'Fortmatic':
        return 'Using your email account'
      case 'Metamask':
      default:
        return 'Using a browser extension'
    }
  }, [logo])

  function handleClick(event: React.MouseEvent<HTMLDivElement>) {
    if (onClick) {
      onClick(event, logo)
    }
  }

  return (
    <div className="walletButton" onClick={handleClick}>
      <div className="walletImage">{src}</div>
      <div className="walletTitle"><h3>{title}</h3></div>
      <div className="walletDescription"><p>{description}</p></div>
    </div>
  )
};
