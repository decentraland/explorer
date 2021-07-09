import React from 'react'
import './GuestButton.css'

export interface WalletButtonProps {
  active?: boolean
  onClick: (providerType: null) => void
}

export const GuestButton: React.FC<WalletButtonProps> = ({ active, onClick }) => {
  function handleClick(event: React.MouseEvent<HTMLAnchorElement>) {
    if (active !== false) {
      event.preventDefault()
      if (onClick) {
        onClick(null)
      }
    }
  }

  return (
    <a
      href="#guest"
      target="_blank"
      rel="noopener noreferrer"
      className={`guestButton ${active ? 'active' : 'inactive'}`}
      onClick={handleClick}
    >
      Play as Guest
    </a>
  )
}
