import React from "react";
import { PassportForm } from "./PassportForm";
import { Modal } from "../common/Modal";
import { PassportAvatar } from "./PassportAvatar";

export interface PassportProps {
  name?: string;
  email?: string;
  onSubmit: any;
  onEditAvatar: any;
}

export const Passport: React.FC<PassportProps> = (props) => (
  <Modal>
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
      <PassportAvatar onEditAvatar={props.onEditAvatar} />
    </div>
    <div
      className="column"
      style={{
        alignSelf: "self-start",
        flex: "1 0 auto",
        padding: "65px 0",
      }}
    >
      <PassportForm
        name={props.name}
        email={props.email}
        onSubmit={props.onSubmit}
      />
    </div>
  </Modal>
);
