import React from "react";
import { PassportForm } from "./PassportForm";
import { Modal } from "../../common/Modal";
import { PassportAvatar } from "./PassportAvatar";

import "./Passport.css";

export interface PassportProps {
  face: string;
  name?: string;
  email?: string;
  onSubmit: any;
  onEditAvatar: any;
}

export const Passport: React.FC<PassportProps> = (props) => (
  <Modal>
    <div className="passport">
      <h2 className="passportTitle">Passport</h2>
      <div className="passportContainer">
        <PassportAvatar face={props.face} onEditAvatar={props.onEditAvatar} />
        <PassportForm
          name={props.name}
          email={props.email}
          onSubmit={props.onSubmit}
        />
      </div>
    </div>
  </Modal>
);
