import React from "react";
import { PassportForm } from "./PassportForm";
import { Modal } from "../../common/Modal";
import { PassportAvatar } from "./PassportAvatar";

import "./Passport.css";

export interface PassportProps {
  face: string;
  name?: string;
  email?: string;
  handleSubmit: any;
  handleCancel: any;
  handleEditAvatar: any;
}

export const Passport: React.FC<PassportProps> = (props) => (
  <Modal handleClose={props.handleCancel}>
    <div className="passport">
      <h2 className="passportTitle">Passport</h2>
      <div className="passportContainer">
        <PassportAvatar
          face={props.face}
          onEditAvatar={props.handleEditAvatar}
        />
        <PassportForm
          name={props.name}
          email={props.email}
          onSubmit={props.handleSubmit}
        />
      </div>
    </div>
  </Modal>
);
