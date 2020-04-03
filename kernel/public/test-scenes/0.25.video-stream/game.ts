import { Entity, engine, Transform, Vector3, VideoShape } from 'decentraland-ecs/src'

const video = new Entity()
video.addComponent(new Transform({ position: new Vector3(8, 1, 8) }))

const videoShape = new VideoShape()
videoShape.url = 'http://localhost:4533/video.mp4'
video.addComponent(videoShape)

engine.addEntity(video)
