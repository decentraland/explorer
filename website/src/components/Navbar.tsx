import React from "react";
import "./Navbar.css";

export const Navbar: React.FC = () => (
  <nav className="nav-bar">
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
