import React, { useState } from "react"
import { ProviderType } from "decentraland-connect/dist/types"
import { WalletSelector } from "./wallet/WalletSelector"
import { LoginHeader } from "./LoginHeader"
import { Spinner } from "../common/Spinner"
import { Avatars } from "../common/Avatars"
import { track } from "../../utils"
import "./EthLogin.css"

export interface EthLoginProps {
  availableProviders: ProviderType[]
  onLogin: (provider: ProviderType | null) => void
  signing: boolean
}

export const EthLogin: React.FC<EthLoginProps> = (props) => {
  const [showWalletSelector, setShowWalletSelector] = useState(false)  

  function handlePlay() {
    track('open_login_popup')
    setShowWalletSelector(true)
  }

  function handleLogin(provider: ProviderType | null) {
    track('click_login_button', { provider_type: provider || 'guest' })
    if (props.onLogin) {
      props.onLogin(provider)
    }
  }

  function handlePlayAsGuest() {
    handleLogin(null)
  }

  return (
    <div className="eth-login">
      <LoginHeader />
      <Avatars />
      <div id="eth-login-confirmation-wrapper">
        {props.signing && <Spinner />}
        {!props.signing && (
          <React.Fragment>
            <button className="eth-login-confirm-button" onClick={handlePlay}>
              Play
            </button>
            <button className="eth-login-guest-button" onClick={handlePlayAsGuest}>
              Play as Guest
            </button>
          </React.Fragment>
        )}
      </div>
      <WalletSelector
        open={showWalletSelector}
        loading={props.signing}
        onLogin={handleLogin}
        availableProviders={props.availableProviders}
        onCancel={() => setShowWalletSelector(false)}
      />
    </div>
  )
}
