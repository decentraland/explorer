import React from "react";
import "./Footer.css";

export const Footer: React.FC = () => (
  <footer className="footer-bar">
    <div className="footer-bar-content">
      <a
        className="footer-link"
        href="https://dcl.gg/discord"
        target="about:blank"
      >
        <img
          alt=""
          className="footer-icon"
          src="images/decentraland-connect/footer/Discord.png"
        />
      </a>
      <a
        className="footer-link"
        href="https://www.reddit.com/r/decentraland/"
        target="about:blank"
      >
        <img
          alt=""
          className="footer-icon"
          src="images/decentraland-connect/footer/Reddit.png"
        />
      </a>
      <a
        className="footer-link"
        href="http://github.com/decentraland"
        target="about:blank"
      >
        <img
          alt=""
          className="footer-icon"
          src="images/decentraland-connect/footer/Git.png"
        />
      </a>
      <a
        className="footer-link"
        href="https://twitter.com/decentraland"
        target="about:blank"
      >
        <img
          alt=""
          className="footer-icon"
          src="images/decentraland-connect/footer/Twitter.png"
        />
      </a>
      <span className="footer-text">© 2020 Decentraland</span>
    </div>
  </footer>
);
