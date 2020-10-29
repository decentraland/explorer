import React from "react";
import "./PassportAvatar.css";

export interface PassportAvatarProps {
  onEditAvatar: any;
}

export const PassportAvatar: React.FC<PassportAvatarProps> = (props) => (
  <div className="PassportAvatar">
    <img
      alt=""
      className="avatar"
      width="180"
      height="180"
      src="https://peer.decentraland.org/content/contents/QmWUT7gPSJXvsscfkERKKKdVpMU4hu2EGuK9RJ72K9tUsU"
    />
    <em>Active since Aug 2020</em>
    <button onClick={props.onEditAvatar}>Edit Avatar</button>
  </div>
);
