import { login, signup, signupAgree, signupForm } from './actions'
import { StoreContainer } from '../store/rootTypes'
import { ensureUnityInterface } from '../renderer'
import { ProfileAsPromise } from '../profiles/ProfileAsPromise'
import { profileToRendererFormat } from '../profiles/transformations/profileToRendererFormat'
import { getCurrentUserId } from './selectors'
import { baseCatalogsLoaded } from '../profiles/selectors'
import { Profile } from '../profiles/types'
import { getUserProfile } from '../comms/peers'

declare const globalThis: StoreContainer

export function setupAuthFlow() {
  const element = document.getElementById('eth-login')
  if (element) {
    element.style.display = 'block'
    const btnSignup = document.getElementById('btnSignUp')
    if (btnSignup) {
      const signupFlow = document.getElementById('signup-flow')
      const signupStep2 = document.getElementById('signup-step2')
      const signupStep3 = document.getElementById('signup-step3')
      const signupStep4 = document.getElementById('signup-step4')
      const btnSignupBack = document.getElementById('btnSignupBack')
      const btnSignupAgree = document.getElementById('btnSignupAgree')
      const btnSignupWallet = document.getElementById('btnSignupWallet')

      const form = document.getElementById('signup-form') as HTMLFormElement

      signupFlow!.style.display = 'none'
      signupStep2!.style.display = 'none'
      signupStep3!.style.display = 'none'
      signupStep4!.style.display = 'none'

      btnSignup.addEventListener('click', () => {
        signupFlow!.style.display = 'block'
        signupStep2!.style.display = 'block'

        const unsubscribe = globalThis.globalStore.subscribe(() => {
          if (baseCatalogsLoaded(globalThis.globalStore.getState())) {
            unsubscribe()
            getLocalProfile()
              .then((profile) => {
                ensureUnityInterface()
                  .then((unityInterface) => {
                    unityInterface.LoadProfile(profileToRendererFormat(profile))
                    unityInterface.ShowAvatarEditorInSignInFlow()
                    unityInterface.ActivateRendering(true)
                  })
                  .catch()
              })
              .catch()
          }
        })
      })

      btnSignupBack!.addEventListener('click', () => {
        signupStep3!.style.display = 'none'
        signupStep2!.style.display = 'block'
      })
      btnSignupAgree!.addEventListener('click', () => {
        console.log('SIGNUP-AGREE')

        globalThis.globalStore.dispatch(signupAgree())

        signupStep3!.style.display = 'none'
        signupStep4!.style.display = 'block'
      })

      btnSignupWallet!.addEventListener('click', () => {
        console.log('SIGNUP-CHOOSE_WALLET')
        globalThis.globalStore.dispatch(signup())
      })

      form!.addEventListener('submit', (event) => {
        event.preventDefault()
        const formData = new FormData(form)
        const name = formData.get('name') as string
        const email = formData.get('email') as string

        console.log('SIGNUP-FORM-DATA: ', { name, email })

        globalThis.globalStore.dispatch(signupForm({ name, email }))

        signupStep2!.style.display = 'none'
        signupStep3!.style.display = 'block'
        return false
      })
    }
    const btnLogin = document.getElementById('eth-login-confirm-button')
    if (btnLogin) {
      btnLogin!.onclick = () => {
        globalThis.globalStore.dispatch(login())
        const unsubscribe = globalThis.globalStore.subscribe(() => {
          if (globalThis.globalStore.getState().session.initialized) {
            element.style.display = 'none'
            unsubscribe()
          }
        })
      }
    }
  }
}

function getLocalProfile(): Promise<Profile> {
  const profile = getUserProfile().profile as Profile | null
  if (profile) {
    return new Promise<Profile>((resolve) => {
      return resolve(profile)
    })
  }
  return ProfileAsPromise(getCurrentUserId(globalThis.globalStore.getState()) ?? '')
}
