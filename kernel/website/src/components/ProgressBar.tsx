import * as React from 'react'

import './ProgressBar.css'

export interface ProgressBarProps {
  percentage: number
}

export const ProgressBar: React.FC<ProgressBarProps> = ({ percentage }) => {
  const progress = {
    width: `${percentage}%`
  }
  return (
    <div className="progress">
      <div className="full" style={progress}>
        {' '}
      </div>
    </div>
  )
}
