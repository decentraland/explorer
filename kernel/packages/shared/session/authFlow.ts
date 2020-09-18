import { login, signup, signupAgree, signupForm, signUpActive } from './actions'
import { StoreContainer } from '../store/rootTypes'
import { ensureUnityInterface } from '../renderer'
import { profileToRendererFormat } from '../profiles/transformations/profileToRendererFormat'
import { baseCatalogsLoaded } from '../profiles/selectors'
import { Avatar, Profile } from '../profiles/types'
import { setLoadingScreenVisible } from '../../unity-interface/dcl'
import { createLocalAuthIdentity } from '../ethereum/provider'
import { getFromLocalStorage } from '../../atomicHelpers/localStorage'
import { ProfileAsPromise } from 'shared/profiles/ProfileAsPromise'

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
      const btnBackToAvatareditor = document.getElementById('btn-signup-edit-avatar')

      const form = document.getElementById('signup-form') as HTMLFormElement

      signupFlow!.style.display = 'none'
      signupStep2!.style.display = 'none'
      signupStep3!.style.display = 'none'
      signupStep4!.style.display = 'none'

      btnSignup.addEventListener('click', () => {
        globalThis.globalStore.dispatch(signUpActive(true))
        GoToAvatarEditor(element)
      })

      btnBackToAvatareditor!.addEventListener('click', () => {
        GoToAvatarEditor(element)
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

      document.querySelector('.btnSignupWallet')!.addEventListener('click', (event: any) => {
        const provider = event.target.getAttribute('rel')
        console.log('SIGNUP-CHOOSE_WALLET: ', provider)
        globalThis.globalStore.dispatch(signup(provider))
      })

      form!.addEventListener('submit', (event) => {
        event.preventDefault()
        const formData = new FormData(form)
        const name = formData.get('name') as string
        const email = formData.get('email') as string

        console.log('SIGNUP-FORM-DATA: ', { name, email })

        globalThis.globalStore.dispatch(signupForm(name, email))

        signupStep2!.style.display = 'none'
        signupStep3!.style.display = 'block'
        return false
      })
    }
    const btnLogin = document.getElementById('eth-login-confirm-button')
    if (btnLogin) {
      const handleLoginClick = (e: any) => {
        const provider = e.target.getAttribute('rel') || 'metamask'
        globalThis.globalStore.dispatch(login(provider))
        const unsubscribe = globalThis.globalStore.subscribe(() => {
          if (globalThis.globalStore.getState().session.initialized) {
            element.style.display = 'none'
            unsubscribe()
          }
        })
      }
      btnLogin!.addEventListener('click', handleLoginClick)
    }
  }
}

export function toggleScreen(screen: 'renderer' | 'signin') {
  const signupFlow = document.getElementById('signup-flow')
  const signupStep2 = document.getElementById('signup-step2')

  if (screen === 'renderer') {
    signupFlow!.style.display = 'none'
    signupStep2!.style.display = 'none'
    document.getElementById('gameContainer')!.setAttribute('style', 'display: block')
  } else {
    signupFlow!.style.display = 'block'
    signupStep2!.style.display = 'block'
    document.getElementById('gameContainer')!.setAttribute('style', 'display: none')
  }
}

function GoToAvatarEditor(element: HTMLElement) {
  element.style.display = 'none'
  toggleScreen('renderer')
  setLoadingScreenVisible(true)

  const unsubscribe = globalThis.globalStore.subscribe(() => {
    if (baseCatalogsLoaded(globalThis.globalStore.getState())) {
      unsubscribe()
      getLocalProfile()
        .then((profile: Profile) => {
          ensureUnityInterface()
            .then((unityInterface) => {
              setLoadingScreenVisible(false)
              unityInterface.LoadProfile(profileToRendererFormat(profile))
              unityInterface.ShowAvatarEditorInSignInFlow()
              console.log(`[SANTI] GO TO AVATAR EDITOR -> ActivateRendering()`)
              unityInterface.ActivateRendering(true)
            })
            .catch()
        })
        .catch()
    }
  })
}

async function getLocalProfile() {
  let profile = getFromLocalStorage('signup_profile') as Profile | null
  if (profile && profile.userId) {
    return profile
  }
  const identity = await createLocalAuthIdentity()
  profile = await ProfileAsPromise(identity.address)
  if (profile) {
    return profile
  }

  let avatar: Avatar = {
    bodyShape: 'dcl://base-avatars/BaseMale',
    snapshots: {
      face: 'QmdYJirtVP61n8AmRzX7FpZ9FzKrcQ8zMi33mjiEKZrXhs',
      face128: 'QmNLneJ2SAV9pEvgGSL3bYAz8PLvHo3Xag7PPghX6NGtZS',
      face256: 'QmNuoogE4r1ho3Bt5hYJMUgr3W6ZY4Jt8Z2Lt7cSvEv5PC',
      body: 'Qmdm6a5kokhfAmTA9kxzyKspY6KSY8CtyvLMRBsviefjjZ'
    },
    eyeColor: '#FFFFFF',
    hairColor: '#FFFFFF',
    skinColor: '#FFFFFF',
    wearables: [
      'dcl://base-avatars/casual_hair_01',
      'dcl://base-avatars/eyebrows_01',
      'dcl://base-avatars/eyes_01',
      'dcl://base-avatars/chin_beard',
      'dcl://base-avatars/green_tshirt',
      'dcl://base-avatars/comfortablepants',
      'dcl://base-avatars/sport_black_shoes',
      'dcl://base-avatars/black_sun_glasses',
      'dcl://base-avatars/mouth_05'
    ]
  }

  return {
    userId: identity.address.toString(),
    name: 'USER_TEST_AVATAR_EDITOR',
    hasClaimedName: false,
    description: '',
    email: '',
    avatar,
    ethAddress: identity.address.toString(),
    inventory: [],
    blocked: [],
    version: 0,
    tutorialStep: 0
  }
}
