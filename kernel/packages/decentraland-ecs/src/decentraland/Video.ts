import { Component, ObservableComponent } from '../ecs/Component'
import { CLASS_ID } from './Components'

/**
 * @public
 */
@Component('engine.videoShape', CLASS_ID.VIDEO_SHAPE)
export class VideoShape extends ObservableComponent {
  @ObservableComponent.field
  id!: string

  @ObservableComponent.field
  url!: string
}
