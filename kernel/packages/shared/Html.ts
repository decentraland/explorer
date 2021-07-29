export default class Html {
  static loopbackAudioElement() {
    return document.getElementById('voice-chat-audio') as HTMLAudioElement | undefined
  }
}
