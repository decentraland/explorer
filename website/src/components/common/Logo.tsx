import React from "react";
import "./Logo.css";
import pngLogo from "../../images/logo-dcl.png";

export const Logo: React.FC = () => (
  <img alt="Decentraland" className="eth-login-logo" src={pngLogo} />
);
