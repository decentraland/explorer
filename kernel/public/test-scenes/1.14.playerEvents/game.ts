import { engine, Entity, Transform, Vector3, onPlayerExpressionObservable, onPositionChangedObservable, onRotationChangedObservable, TextShape, onEnterSceneObservable, BoxShape, Quaternion } from 'decentraland-ecs/src'

const box = new Entity();
box.addComponent(new BoxShape());
box.addComponent(new Transform({ position: new Vector3(8, 2, 8), scale: new Vector3(0.25, 0.25, 1) }));
engine.addEntity(box);

//Create entity and assign shape
const text = new Entity()
const shape = new TextShape("Null...")
text.addComponent(shape)

text.addComponent(new Transform({
  position: new Vector3(8, 1, 8),
  scale: new Vector3(-0.25, 0.25, -0.25)
}))

engine.addEntity(text);

onEnterSceneObservable.add(({ userId }) => {
  shape.value = "Enter: " + userId
})

onPlayerExpressionObservable.add(({ userId, expressionId }) => {
  shape.value = "Expression: " + userId + " - " + expressionId
})

onPositionChangedObservable.add((eventData) => {
  text.getComponent(Transform).lookAt(new Vector3(eventData.position.x, eventData.position.y, eventData.position.z))
})
onRotationChangedObservable.add((eventData) => {
  box.getComponent(Transform).rotation = new Quaternion(eventData.quaternion.x, eventData.quaternion.y, eventData.quaternion.z, eventData.quaternion.w)
})