import { log, engine, Entity, Transform, Vector3, onSceneReadyObservable, TextShape } from 'decentraland-ecs'

//Create entity and assign shape
const text = new Entity()
const shape = new TextShape('Loading scene...')
text.addComponent(shape)

text.addComponent(
  new Transform({
    position: new Vector3(8, 2, 8),
    scale: new Vector3(1, 1, 1)
  })
)

engine.addEntity(text)

onSceneReadyObservable.add(() => {
  log('onSceneReadyObservable')
  shape.value = 'Scene ready!'
})
