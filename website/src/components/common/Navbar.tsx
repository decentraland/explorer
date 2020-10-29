import React from "react";
import "./Navbar.css";
import logo from "../../images/logo.png";

export const Navbar: React.FC = () => (
  <nav className="nav-bar">
    <img src={logo} alt="" className="nav-logo" />
    <div className="nav-bar-content">
      <div className="nav-text nav-need-support">
        <span>Need support?</span>
      </div>
      <a
        className="nav-discord"
        href="https://dcl.gg/discord"
        target="about:blank"
      >
        <img
          alt="Discord"
          className="nav-discord-img"
          src="images/decentraland-connect/Discord.png"
        />
        <span className="nav-text nav-discord-text">JOIN OUR DISCORD</span>
      </a>
    </div>
  </nav>
);
