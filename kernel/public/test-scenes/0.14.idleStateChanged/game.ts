import { log, engine, Entity, Transform, Vector3, onIdleStateChangedObservable, TextShape } from 'decentraland-ecs'

// AFK Mode Changed
//Create entity and assign shape
const text = new Entity()
const shape = new TextShape("You're not AFK")
text.addComponent(shape)

text.addComponent(
  new Transform({
    position: new Vector3(8, 2, 8),
    scale: new Vector3(1, 1, 1)
  })
)

engine.addEntity(text)

onIdleStateChangedObservable.add(({ isIdle }) => {
  log('onIdleStateChangedObservable', isIdle)
  if (isIdle) shape.value = "YOU'RE AFK!"
  else shape.value = "You're not AFK!"
})
