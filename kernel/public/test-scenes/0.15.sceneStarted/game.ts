import { log, engine, Entity, Transform, Vector3, onSceneStartObservable, TextShape } from 'decentraland-ecs/src'

//Create entity and assign shape
const text = new Entity()
const shape = new TextShape("Loading scene...")
text.addComponent(shape)

text.addComponent(new Transform({
  position: new Vector3(8, 2, 8),
  scale: new Vector3(1, 1, 1),
}))

engine.addEntity(text);

onSceneStartObservable.add(() => {
  log("onSceneStartObservable")
  shape.value = "Scene ready!"
})