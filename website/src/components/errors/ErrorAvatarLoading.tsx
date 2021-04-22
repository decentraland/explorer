import React from "react";

import "./errors.css";
import errorImage from "../../images/errors/error-robotmobile.png";

export const ErrorAvatarLoading: React.FC = () => (
  <div id="error-avatarerror" className="error-container">
    <div className="error-background" />
    <div className="errormessage">
      <div className="errortext col">
        <div className="error">Oops...</div>
        <div className="communicationslink">
          There was a problem loading your avatar
        </div>
        <div className="givesomedetailof">
          It seems that the body shape of your avatar could not be loaded correctly.
          <br />
          <br />
          Please try again later, or reach out to us at{" "}
          <a href="mailto:hello@decentraland.org">
            hello@decentraland.org
          </a>
        </div>
      </div>
      <div className="errorimage col">
        <img alt="" className="error-image" src={errorImage} />
      </div>
    </div>
  </div>
);
