import { login, signup, signupAgree, signupForm, signUpActive, authClearError } from './actions'
import { StoreContainer } from '../store/rootTypes'
import { ensureUnityInterface } from '../renderer'
import { profileToRendererFormat } from '../profiles/transformations/profileToRendererFormat'
import { baseCatalogsLoaded } from '../profiles/selectors'
import { Avatar, Profile } from '../profiles/types'
import { setLoadingScreenVisible } from '../../unity-interface/dcl'
import { createLocalAuthIdentity } from '../ethereum/provider'
import { getFromLocalStorage } from '../../atomicHelpers/localStorage'
import { AuthError } from './types'
import { getAuthError, isSignUpActive } from './selectors'

declare const globalThis: StoreContainer
enum AuthSection {
  SIGN_IN = 'eth-login',
  SING_UP = 'signup-flow',
  SIGN_UP_EDITOR = 'avatar-editor',
  SIGN_UP_STEP_2 = 'signup-step2',
  SIGN_UP_STEP_3 = 'signup-step3',
  SIGN_UP_STEP_4 = 'signup-step4'
}
let modals: Map<string, Modal>

export function setupAuthFlow() {
  modals = setupErrorModals()
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
        switchTo(AuthSection.SIGN_UP_EDITOR)
      })

      btnBackToAvatareditor!.addEventListener('click', () => {
        switchTo(AuthSection.SIGN_UP_EDITOR, AuthSection.SIGN_UP_STEP_2)
      })
      btnSignupBack!.addEventListener('click', () => {
        switchTo(AuthSection.SIGN_UP_STEP_2, AuthSection.SIGN_UP_STEP_2)
      })
      btnSignupAgree!.addEventListener('click', () => {
        globalThis.globalStore.dispatch(signupAgree())
        switchTo(AuthSection.SIGN_UP_STEP_4, AuthSection.SIGN_UP_STEP_3)
      })

      document.querySelector('.btnSignupWallet')!.addEventListener('click', (event: any) => {
        const provider = event.target.getAttribute('rel')
        const unsubscribe = globalThis.globalStore.subscribe(() => {
          const error = getAuthError(globalThis.globalStore.getState())
          if (modals.has(error)) {
            unsubscribe()
            globalThis.globalStore.dispatch(authClearError())
            modals.get(error)!.open()
          }
          if (!isSignUpActive(globalThis.globalStore.getState())) {
            unsubscribe()
          }
        })
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
          const error = getAuthError(globalThis.globalStore.getState())
          if (modals.has(error)) {
            unsubscribe()
            globalThis.globalStore.dispatch(authClearError())
            modals.get(error)!.open()
            return
          }
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

  const identity = await createLocalAuthIdentity()
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

interface ModalClickListener {
  (e: Event, modal: Modal): void
}

class Modal {
  private readonly container: HTMLElement

  constructor(id: string, onAccept: ModalClickListener, onCancel?: ModalClickListener) {
    this.container = document.getElementById(id) as HTMLElement
    if (!this.container) {
      throw Error('Modal element does not exist')
    }
    const btnAccept = this.container.querySelector(`.btnAccept`)
    if (btnAccept && onAccept) {
      btnAccept.addEventListener('click', (event) => onAccept(event, this))
    }
    const btnCancel = this.container.querySelector(`.btnCancel`)
    if (btnCancel && onCancel) {
      btnCancel.addEventListener('click', (event) => onCancel(event, this))
    }
  }

  open() {
    this.container.style.display = 'block'
  }

  close() {
    this.container.style.display = 'none'
  }
}

function setupErrorModals() {
  const tosNotAccepted = new Modal(AuthError.TOS_NOT_ACCEPTED, (event, modal) => modal.close())
  const accountNotFound = new Modal(AuthError.ACCOUNT_NOT_FOUND, (event, modal) => modal.close())

  const profileDoesntExist = new Modal(
    AuthError.PROFILE_DOESNT_EXIST,
    (event, modal) => {
      modal.close()
      switchTo(AuthSection.SIGN_UP_EDITOR)
    },
    (event, modal) => modal.close()
  )
  const profileAlreadyExists = new Modal(
    AuthError.PROFILE_ALREADY_EXISTS,
    (event, modal) => {
      modal.close()
      switchTo(AuthSection.SIGN_IN, AuthSection.SIGN_UP_STEP_4)
    },
    (event, modal) => modal.close()
  )
  return new Map<string, Modal>([
    [AuthError.TOS_NOT_ACCEPTED, tosNotAccepted],
    [AuthError.ACCOUNT_NOT_FOUND, accountNotFound],
    [AuthError.PROFILE_DOESNT_EXIST, profileDoesntExist],
    [AuthError.PROFILE_ALREADY_EXISTS, profileAlreadyExists]
  ])
}

function switchTo(section: AuthSection, from?: AuthSection) {
  const signInContainer = document.getElementById(AuthSection.SIGN_IN)
  const signUpContainer = document.getElementById(AuthSection.SING_UP)
  // close all modals
  modals.forEach((m) => m.close())
  ;[AuthSection.SIGN_UP_STEP_2, AuthSection.SIGN_UP_STEP_3, AuthSection.SIGN_UP_STEP_4].map((s) => {
    const e = document.getElementById(s)
    e ? (e.style.display = 'none') : null
  })
  switch (section) {
    case AuthSection.SIGN_IN: {
      globalThis.globalStore.dispatch(signUpActive(false))
      signUpContainer ? (signUpContainer.style.display = 'none') : null
      signInContainer ? (signInContainer.style.display = 'block') : null
      return
    }
    case AuthSection.SIGN_UP_EDITOR: {
      globalThis.globalStore.dispatch(signUpActive(true))
      signInContainer ? (signInContainer.style.display = 'none') : null
      signUpContainer ? (signUpContainer.style.display = 'none') : null
      const fromElement = from ? document.getElementById(from) : null
      fromElement ? (fromElement.style.display = 'none') : null
      GoToAvatarEditor(signInContainer!)
      return
    }
  }

  const sectionTo = document.getElementById(section)
  const fromElement = from ? document.getElementById(from) : null
  fromElement ? (fromElement.style.display = 'none') : null
  sectionTo ? (sectionTo.style.display = 'block') : null
}
