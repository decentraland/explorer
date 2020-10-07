import * as React from 'react'
import './Errors.css'

export const ErrorFatal: React.FC = () => (
  <div id="error-fatal" className="error-container hidden-error">
    <div className="error-background" />
    <div className="errormessage">
      <div className="errortext col">
        <div className="error">Error</div>
        <div className="communicationslink">An unexpected error ocurred when loading the world</div>
        <div className="givesomedetailof">
          This might be just temporary. <br />
          <br /> Please try reloading, and if the problem persists,
          <br />
          feel free to reach out to us at <br />{' '}
          <a href="mailto:developers@decentraland.org">developers@decentraland.org</a>
          <br />
          <br />
          Thank you for helping us improve!
        </div>
        <div className="cta">
          <button
            className="retry"
            onClick={() => {
              window.location.reload()
            }}
          >
            Reload
          </button>
        </div>
      </div>
      <div className="errorimage col">
        <div className="imagewrapper">
          <img alt="" className="error-image" src="images/robots/error-robotdown.png" />
        </div>
      </div>
    </div>
  </div>
)
