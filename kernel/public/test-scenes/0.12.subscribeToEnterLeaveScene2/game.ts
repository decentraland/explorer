import {
  log,
  engine,
  Entity,
  BoxShape,
  Material,
  Color3,
  Transform,
  Vector3,
  onEnterSceneObservable,
  onLeaveSceneObservable,
  onCameraModeChangedObservable,
  Camera,
  CameraMode
} from 'decentraland-ecs'

//Create entity and assign shape
const floor = new Entity()
floor.addComponent(new BoxShape())

//Create material and configure its fields
const floorMaterial = new Material()
floorMaterial.albedoColor = Color3.Gray()

//Assign the material to the entity
floor.addComponent(floorMaterial)

floor.addComponent(
  new Transform({
    position: new Vector3(8, 0, 16),
    scale: new Vector3(16, 0.1, 32)
  })
)

engine.addEntity(floor)

onEnterSceneObservable.add(({ userId }) => {
  floorMaterial.albedoColor = Color3.Red()
  log('onEnterSceneObservable: ', userId, ' - CameraMode: ', Camera.instance.cameraMode)
})

onLeaveSceneObservable.add(({ userId }) => {
  floorMaterial.albedoColor = Color3.Gray()
  log('onLeaveSceneObservable: ', userId)
})

// Camera Mode
//Create entity and assign shape
const box = new Entity()
box.addComponent(new BoxShape())

//Create material and configure its fields
const boxMaterial = new Material()
boxMaterial.albedoColor = Color3.Gray()

//Assign the material to the entity
box.addComponent(boxMaterial)

box.addComponent(
  new Transform({
    position: new Vector3(8, 2, 8),
    scale: new Vector3(1, 1, 1)
  })
)

engine.addEntity(box)

const setFloorMaterial = (cameraMode: CameraMode) => {
  if (cameraMode == CameraMode.FirstPerson) boxMaterial.albedoColor = Color3.Red()
  else if (cameraMode == CameraMode.ThirdPerson) boxMaterial.albedoColor = Color3.Blue()
}

onCameraModeChangedObservable.add(({ cameraMode }) => {
  log('onCameraModeChangedObservable', cameraMode)
  setFloorMaterial(cameraMode)
})

// Init
setFloorMaterial(Camera.instance.cameraMode)
