import {BoxShape, engine, Entity, log, moveTo, OnPointerUp, Transform, Vector3} from 'decentraland-ecs/src'

const e = new Entity()
e.addComponent(new BoxShape())
e.addComponent(new Transform({ position: new Vector3(4, 0, 6) }))
e.addComponent(
  new OnPointerUp((e) => {
    log('clicked: ', e)
    moveTo({x: 10, y: 0, z: 10}, {x: 4, y: 1, z: 6})
  })
)

engine.addEntity(e)
