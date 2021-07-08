import React from "react"
import { connect } from "react-redux"
import { StoreType } from "../../state/redux"
import "./Overlay.css"

const mapStateToProps = (state: StoreType) => {
  return {
    show: false, // (!state.loading.error && state.loading.showLoadingScreen) ||
  }
}

export interface OverlayProps {
  show: boolean
}

export const Overlay: React.FC<OverlayProps> = (props) => (
  <React.Fragment>{props.show && <div id="overlay" />}</React.Fragment>
)

export default connect(mapStateToProps)(Overlay)
