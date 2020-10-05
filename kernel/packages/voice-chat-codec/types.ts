export enum RequestTopic {
  ENCODE = 'ENCODE',
  DECODE = 'DECODE',
  DESTROY_ENCODER = 'DESTROY_ENCODER',
  DESTROY_DECODER = 'DESTROY_ENCODER'
}

export enum WorkletRequestTopic {
  ENCODE = 'ENCODE',
  PAUSE = 'PAUSE',
  RESUME = 'RESUME',
  ON_PAUSED = 'ON_PAUSED',
  ON_RECORDING = 'ON_RECORDING'
}

export enum ResponseTopic {
  ENCODE = 'ENCODE_OUTPUT',
  DECODE = 'DECODE_OUTPUT'
}

export type VoiceChatWorkerRequest = { topic: RequestTopic } & any
export type VoiceChatWorkerResponse = { topic: ResponseTopic } & any
