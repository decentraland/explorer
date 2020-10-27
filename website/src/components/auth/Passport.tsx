import React from "react";

export interface PassportProps {
  show: boolean;
}

export const Passport: React.FC<PassportProps> = (props) => {
  return props.show ? (
    <div className="popup-container">
      <div className="popup big">
        <div className="column" style={{ flex: "0 0 100%" }}>
          <h2>Passport</h2>
        </div>
        <div
          className="column center"
          style={{
            alignSelf: "self-start",
            flex: "0 0 400px",
            padding: "65px 110px 65px",
          }}
        >
          <img
            alt=""
            className="avatar"
            width="180"
            height="180"
            src="https://peer.decentraland.org/content/contents/QmWUT7gPSJXvsscfkERKKKdVpMU4hu2EGuK9RJ72K9tUsU"
          />
          <em
            style={{ margin: "22px 0", textAlign: "center", fontWeight: 300 }}
          >
            Active since Aug 2020
          </em>
          <button id="btn-signup-edit-avatar" className="button secondary">
            Edit Avatar
          </button>
        </div>
        <div
          className="column"
          style={{
            alignSelf: "self-start",
            flex: "1 0 auto",
            padding: "65px 0",
          }}
        >
          <form id="signup-form" method="POST">
            <div>
              <div>*required field (you can edit it later)</div>
              <label>Name your avatar</label>
              <input
                id="signup-avatar-name"
                type="text"
                name="name"
                placeholder="your avatar name"
              />
            </div>
            <div>
              <label>Let's stay in touch</label>
              <input
                id="signup-avatar-email"
                type="text"
                name="email"
                placeholder="enter your email"
              />
            </div>
            <div>
              <button type="submit" className="button primary center">
                Next
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  ) : null;
};
