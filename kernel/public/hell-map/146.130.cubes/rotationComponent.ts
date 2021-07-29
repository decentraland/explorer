import { Component, Vector3 } from 'decentraland-ecs'

@Component('rotationComponent')
export class RotationComponent {
  rotAxis: Vector3
  rotSpd: number

  constructor(axis: Vector3, spd: number) {
    this.rotAxis = axis
    this.rotSpd = spd
    //log("created rotationcomponent with values axis: " + axis + " spd: " + spd);
  }
}
