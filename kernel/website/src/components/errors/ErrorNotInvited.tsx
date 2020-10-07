import * as React from 'react'
import './Errors.css'

export const ErrorNotInvited: React.FC = () => (
  <div id="error-notinvited" className="error-container hidden-error">
    <div className="error-background" />
    <div className="errormessage">
      <div className="errortext col">
        <div className="error">Oops...</div>
        <div className="communicationslink">The Explorer is in Private Beta</div>
        <div className="givesomedetailof">
          Your account is not in the beta testing group. Only users with a claimed name have access right now. <br />
          <br />
          Stay tuned! Genesis City opens its doors on February 20th.
        </div>
      </div>
      <div className="errorimage col">
        <img alt="" className="error-image" src="images/robots/error-robotmobile.png" />
      </div>
    </div>
  </div>
)
