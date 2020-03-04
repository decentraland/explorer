import { getFromLocalStorage } from 'atomicHelpers/localStorage'
export const getUserProfile = () => getFromLocalStorage('dcl-profile') || {}
