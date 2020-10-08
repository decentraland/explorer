import React from "react";
import "./Errors.css";

export const ErrorNewLogin: React.FC = () => (
  <div id="error-newlogin" className="error-container">
    <div className="error-background" />
    <div className="errormessage">
      <div className="errortext col">
        <div className="error communicationslink">
          Another session was detected
        </div>
        <div className="givesomedetailof">
          It seems that the explorer was opened with your account from another
          device, browser, or tab.
          <br />
          Please, close the prior session and click "Reload" to explore the
          world in this window.
        </div>
        <div className="cta">
          <button
            className="retry"
            onClick={() => {
              window.location.reload();
            }}
          >
            Reload
          </button>
        </div>
      </div>
      <div className="errorimage col">
        <div className="imagewrapper">
          <img
            alt=""
            className="error-image"
            src="images/robots/robotsmiling.png"
          />
        </div>
      </div>
    </div>
  </div>
);
