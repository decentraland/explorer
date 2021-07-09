import React from 'react'

import './errors.css'
import errorImage from '../../images/errors/error-robotmobile.png'

export const ErrorAvatarLoading: React.FC = () => (
  <div id="error-avatarerror" className="error-container">
    <div className="error-background" />
    <div className="errormessage">
      <div className="errortext col">
        <div className="error">Oops...</div>
        <div className="communicationslink">
          There was a technical issue and we were unable to retrieve your avatar information
        </div>
        <div className="givesomedetailof">
          Please try again later, and if the problem persists you can contact us through the Discord channel or at{' '}
          <a href="mailto:hello@decentraland.org">hello@decentraland.org</a>
        </div>
      </div>
      <div className="errorimage col">
        <img alt="" className="error-image" src={errorImage} />
      </div>
    </div>
  </div>
)
