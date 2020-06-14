import { unityInterface } from './dcl'
export function setupPointerLock() {
  document.addEventListener('pointerlockchange', e => {
    if (!document.pointerLockElement) {
      unityInterface.UnlockCursor()
    }
  })
}
