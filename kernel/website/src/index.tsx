import * as React from 'react'
import * as ReactDOM from 'react-dom'
import { Provider } from 'react-redux'
import { getKernelStore } from './store'
import * as serviceWorker from './serviceWorker'
import { App } from './components/App'

ReactDOM.render(
  <React.StrictMode>
    <Provider store={getKernelStore()}>
      <App />
    </Provider>
  </React.StrictMode>,
  document.getElementById('root')
)

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister()
