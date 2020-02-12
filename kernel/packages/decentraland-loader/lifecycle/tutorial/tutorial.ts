export let _tutorialEnabled = false

export enum tutorialStepId {
  NONE = 0,
  INITIAL_SCENE = 1,
  GENESIS_PLAZA = 2,
  CHAT_AND_AVATAR_EXPRESSIONS = 3,
  FINISHED = 99
}

export function setTutorialEnabled(v: boolean) {
  _tutorialEnabled = v
}

let teleportCount: number = 0

export function isTutorial(): boolean {
  return teleportCount <= 1 && _tutorialEnabled
}

export function onTutorialTeleport() {
  teleportCount++
}

