import * as React from "react"
import * as ReactDOM from "react-dom"
import { Provider } from "react-redux"
import App from "./components/App"
import { restoreConnection } from "./eth/provider"
import { disableAnalytics, identifyUser, trackEvent } from "./integration/analytics"
import { injectKernel } from "./kernel-loader"
import { setKernelAccountState, setKernelError, setKernelLoaded, setRendererLoading } from "./state/actions"
import { store } from "./state/redux"

let INITIAL_RENDER = true

async function initWebsite() {
  const container = document.getElementById("gameContainer") as HTMLDivElement

  const kernel = await injectKernel({
    container,
    kernelOptions: {
      baseUrl: new URL(`${process.env.PUBLIC_URL}/.`, global.location.toString()).toString(),
      version: performance.now().toString(),
    },
    rendererOptions: {
      baseUrl: new URL(`${process.env.PUBLIC_URL}/unity-renderer/`, global.location.toString()).toString(),
      version: performance.now().toString(),
    },
  })

  kernel.trackingEventObservable.add(({ eventName, eventData }) => {
    trackEvent(eventName, eventData)
  })

  kernel.accountStateObservable.add((account) => {
    if (account.identity) {
      identifyUser(account.identity.address)
    }
    store.dispatch(setKernelAccountState(account))
  })

  kernel.signUpObservable.add(({ email }) => {
    identifyUser(email)
  })

  kernel.errorObservable.add((error) => {
    store.dispatch(setKernelError(error))
    if(error.level == "fatal"){
      disableAnalytics()
    }
  })
  kernel.loadingProgressObservable.add((event) => store.dispatch(setRendererLoading(event)))

  store.dispatch(setKernelLoaded(kernel))
}

async function initLogin() {
  const provider = await restoreConnection()

  if (provider) {
    console.log("got previous provider")
  }
}

initLogin().catch((error) => {
  console.error(error)
})

initWebsite().catch((error) => {
  console.error(error)
  store.dispatch(setKernelError({ error }))
})

ReactDOM.render(
  <React.StrictMode>
    <Provider store={store}>
      <App />
    </Provider>
  </React.StrictMode>,
  document.getElementById('root'),
  () => {
    if (INITIAL_RENDER) {
      INITIAL_RENDER = false
      const initial = document.getElementById('root-loading')
      if (initial) {
        initial.style.opacity = '0'
        setTimeout(() => {
          initial.remove()
        }, 300)
      }
    }
  }
)
