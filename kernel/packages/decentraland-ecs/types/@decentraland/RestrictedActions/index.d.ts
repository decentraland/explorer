declare module '@decentraland/RestrictedActions' {
  /**
   * move player to a position inside the scene
   *
   * @param position PositionType
   * @param cameraTarget PositionType
   */
  export function movePlayerTo(newPosition: PositionType, cameraTarget?: PositionType): Promise<void>

  export type PositionType = { x: number; y: number; z: number }

  /**
   * trigger an emote on the current player
   *
   * @param emote the emote to perform
   */
  export function triggerEmote(emote: Emote): Promise<void>

  export type Emote = {
    predefined: PredefinedEmote
  }

  export type PredefinedEmote = 'WAVE' | 'FIST_PUMP' | 'ROBOT' | 'RAISE_HAND' | 'CLAP' | 'MONEY' | 'KISS'
}