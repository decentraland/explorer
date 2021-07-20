export default class Html {
  static loopbackAudioElement() {
    return document.getElementById('voice-chat-audio') as HTMLAudioElement | undefined
  }

  static hideTeleportAnimation() {
    document.getElementById('gameContainer')!.setAttribute('style', 'background: #151419')
    document.body.setAttribute('style', 'background: #151419')
  }
}
