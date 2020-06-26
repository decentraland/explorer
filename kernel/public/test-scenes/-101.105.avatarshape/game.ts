import { engine, Entity, Transform, Vector3, OnPointerDown, ActionButton, BoxShape
  , executeTask
  // , Vector2, Material, PlaneShape, Quaternion, TransparencyMode, Texture
  // , AvatarShape
 } from 'decentraland-ecs/src'
import { Mannequin } from "./mannequin";

const spawnerEntity = new Entity()
const spawnerTransform = new Transform()
spawnerTransform.position.set(8, 1, 8)
spawnerEntity.addComponentOrReplace(spawnerTransform)

const spawnerBox = new BoxShape()
spawnerEntity.addComponentOrReplace(spawnerBox)

let onClickComponent = new OnPointerDown(
  e => {
    SpawnAvatar();
  }, { button: ActionButton.POINTER, hoverText: "Spawn!", distance: 100 })

spawnerEntity.addComponent(onClickComponent)

engine.addEntity(spawnerEntity)

function SpawnAvatar() {
  executeTask(async () => {
    const mannequin = new Mannequin("TestAvatar");
    mannequin.setPosition(new Vector3(Math.random() * 15, 0, Math.random() * 15));
  });

  // let entity = new Entity()

  // let transform = new Transform()
  // transform.position = new Vector3(Math.random() * 15, 0, Math.random() * 15)
  // entity.addComponent(transform)

  // // let shape = AvatarShape.Dummy()
  // let shape = new AvatarShape()
  // // shape.useDummyModel = true
  // entity.addComponent(shape)

  // engine.addEntity(entity)
}
