function createObjectEntity() : Entity {
  const newEntity = new Entity()
  newEntity.addComponent(new Transform({ position: new Vector3(0, 0, 0), scale: new Vector3(0.2, 0.2, 0.2) }))

  const box = new BoxShape()
  box.withCollisions = false
  newEntity.addComponent(box)

  engine.addEntity(newEntity)

  return newEntity
}

class UpdateObjectRotation {
  update(dt: number) {
    newEntityTransform.rotate(Vector3.Up(), dt * 100)
  }
}

class UpdateObjectPosition {
  update() {
    newEntityTransform.position = new Vector3(camera.position.x, camera.position.y + 0.5, camera.position.z)
  }
}

const camera = Camera.instance
const newEntity = createObjectEntity()
const newEntityTransform = newEntity.getComponent(Transform)
engine.addSystem(new UpdateObjectRotation())
engine.addSystem(new UpdateObjectPosition())
