import React from "react";

import "./errors.css";
import errorImage from "../../images/errors/error-robotmobile.png";

export const ErrorNotSupported: React.FC = () => (
  <div id="error-notsupported" className="error-container">
    <div className="error-background" />
    <div className="errormessage">
      <div className="errortext col">
        <div className="error">Error</div>
        <div className="communicationslink">
          Your browser is not supported
        </div>
        <div className="givesomedetailof">
          While it may be technically possible to use another browser,
          we recommend <a href="https://www.google.com/chrome/?brand=BNSD&gclid=Cj0KCQjw5uWGBhCTARIsAL70sLJsOBpT9ZDBOEtFx49Ya-nc0iCR50klEV8iLNoOdkeVtVly8x9_gXUaAhSdEALw_wcB&gclsrc=aw.ds" target="_blank" rel="noreferrer">Chrome</a> or <a href="https://www.mozilla.org/en-US/firefox/new/" target="_blank" rel="noreferrer">Firefox</a> to ensure optimal performance.
        </div>
      </div>
      <div className="errorimage col">
        <img alt="" className="error-image" src={errorImage} />
      </div>
    </div>
  </div >
);
