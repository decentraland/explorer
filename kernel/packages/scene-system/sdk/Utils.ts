import { CLASS_ID, Transform } from "decentraland-ecs/src"
import { PB_Transform, PB_Vector3, PB_Quaternion } from "../../shared/proto/engineinterface_pb"
import { Vector3 } from "decentraland-ecs"

const VECTOR3_MEMBER_CAP = 1000000 // Value measured when genesis plaza glitch triggered a physics engine breakdown
const pbTransform: PB_Transform = new PB_Transform()
const pbPosition: PB_Vector3 = new PB_Vector3()
const pbRotation: PB_Quaternion = new PB_Quaternion()
const pbScale: PB_Vector3 = new PB_Vector3()

export function generatePBObject(classId: CLASS_ID, json: string): string {
  if (classId === CLASS_ID.TRANSFORM) {
    const transform: Transform = JSON.parse(json)
    return serializeTransform(transform)
  }

  return json
}

export function generatePBObjectJSON(classId: CLASS_ID, json: any): string {
  if (classId === CLASS_ID.TRANSFORM) {
    return serializeTransform(json)
  }
  return JSON.stringify(json)
}

function serializeTransform(transform: Transform): string {
  // Position
  // If we don't cap these vectors, scenes may trigger a physics breakdown when messaging enormous values
  const cappedVector = new Vector3(Math.fround(transform.position.x),
                                  Math.fround(transform.position.y),
                                  Math.fround(transform.position.z))
  capVector(cappedVector, VECTOR3_MEMBER_CAP)
  pbPosition.setX(cappedVector.x)
  pbPosition.setY(cappedVector.y)
  pbPosition.setZ(cappedVector.z)

  // Rotation
  pbRotation.setX(transform.rotation.x)
  pbRotation.setY(transform.rotation.y)
  pbRotation.setZ(transform.rotation.z)
  pbRotation.setW(transform.rotation.w)

  // Scale
  cappedVector.set(Math.fround(transform.scale.x),
                    Math.fround(transform.scale.y),
                    Math.fround(transform.scale.z))
  capVector(cappedVector, VECTOR3_MEMBER_CAP)
  pbScale.setX(cappedVector.x)
  pbScale.setY(cappedVector.y)
  pbScale.setZ(cappedVector.z)

  // Apply values
  pbTransform.setPosition(pbPosition)
  pbTransform.setRotation(pbRotation)
  pbTransform.setScale(pbScale)

  let arrayBuffer: Uint8Array = pbTransform.serializeBinary()
  return btoa(String.fromCharCode(...arrayBuffer))
}

function capVector(targetVector: Vector3, cap: number) {
  if (Math.abs(targetVector.x) > cap) {
    targetVector.x = cap * Math.sign(targetVector.x)
  }

  if (Math.abs(targetVector.y) > cap) {
    targetVector.y = cap * Math.sign(targetVector.y)
  }

  if (Math.abs(targetVector.z) > cap) {
    targetVector.z = cap * Math.sign(targetVector.z)
  }
}
