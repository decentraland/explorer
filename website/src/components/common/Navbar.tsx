import React from "react";
import { Logo } from "./Logo";
import { Isologotipo } from "./Isologotipo";
import { Discord } from "./Icon.tsx/Discord";
import "./Navbar.css";

export type NavbarProps = {
  full?: boolean,
  onClickLogo?: (event: React.MouseEvent<SVGElement>) => void
}

export const Navbar = (props: NavbarProps) => (
  <nav className="nav-bar">
    {!props.full && <Isologotipo onClick={props.onClickLogo} />}
    {!!props.full && <a href="https://decentraland.org/">
      <Logo onClick={props.onClickLogo} />
    </a>}
    {!!props.full && <div className="nav-bar-content">
      <div className="nav-text">
        <span>Need support?</span>
      </div>
      <a
        className="nav-discord"
        href="https://dcl.gg/discord"
        target="about:blank"
      >
        <Discord />
        <div>Join our discord</div>
      </a>
    </div>}
  </nav>
);
