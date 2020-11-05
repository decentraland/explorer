import React from "react";
import { Avatars } from "./Avatars";
import "./Modal.css";

export interface ModalProps {
  withAvatars?: boolean;
  handleClose?: () => void;
}

export const Modal: React.FC<ModalProps> = ({ handleClose, withAvatars, children }) => {
  return (
    <div className={'popup-container' + (withAvatars && ' with-avatars' || '')}>
      <div className="popup">
        {handleClose && <div className="close" onClick={handleClose} />}
        {children}
      </div>
      {withAvatars && <Avatars />}
    </div>
  );
};
