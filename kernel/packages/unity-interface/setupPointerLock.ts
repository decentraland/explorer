import { unityInterface } from './unityInterface'
export function setupPointerLock() {
  document.addEventListener('pointerlockchange', e => {
    if (!document.pointerLockElement) {
      unityInterface.UnlockCursor()
    }
  })
}
