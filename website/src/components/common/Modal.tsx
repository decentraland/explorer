import React from "react";
import "./Modal.css";

export interface ModalProps {
  handleClose?: () => void;
}

export const Modal: React.FC<ModalProps> = ({ handleClose, children }) => {
  return (
    <div className="popup-container">
      <div className="popup big">
        {handleClose && <div className="close" onClick={handleClose} />}
        {children}
      </div>
    </div>
  );
};
