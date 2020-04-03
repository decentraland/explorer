import { Entity, engine, Transform, Vector3, BoxShape, VideoShape } from 'decentraland-ecs/src'

const cube = new Entity()
cube.addComponent(new BoxShape())
cube.addComponent(new Transform({ position: new Vector3(8, 1, 8) }))

const videoShape = new VideoShape()
videoShape.url = 'http://localhost:4533/video.mp4'
cube.addComponent(videoShape)

engine.addEntity(cube)
