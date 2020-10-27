import React from "react";

export interface PassportAvatarProps {
  onEditAvatar: any;
}

export const PassportAvatar: React.FC<PassportAvatarProps> = (props) => (
  <React.Fragment>
    <img
      alt=""
      className="avatar"
      width="180"
      height="180"
      src="https://peer.decentraland.org/content/contents/QmWUT7gPSJXvsscfkERKKKdVpMU4hu2EGuK9RJ72K9tUsU"
    />
    <em
      style={{
        margin: "22px 0",
        textAlign: "center",
        fontWeight: 300,
      }}
    >
      Active since Aug 2020
    </em>
    <button
      id="btn-signup-edit-avatar"
      className="button secondary"
      onClick={props.onEditAvatar}
    >
      Edit Avatar
    </button>
  </React.Fragment>
);
