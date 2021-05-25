import { engine, BoxShape, Entity, Animator, ActionButton, OnPointerDown, Transform, Vector3, GLTFShape, AnimationState } from 'decentraland-ecs/src'

const createButton = (pos : number, hoverText : string, onClick : Function) => {
  // Create box shape
  const boxShapeEntity = new Entity()

  const box = new BoxShape()

  boxShapeEntity.addComponent(
    new OnPointerDown(
      e => {
        onClick()
      },
      { button: ActionButton.POINTER, hoverText: hoverText }
    )
  )

  boxShapeEntity.addComponent(
    new Transform({
      position: new Vector3(6, 0.5, pos),
      scale: new Vector3(0.2, 0.2, 0.2)
    })
  )
  boxShapeEntity.addComponent(box)

  engine.addEntity(boxShapeEntity)
}

// Dog
const dog = new Entity()
const dogAnimator = new Animator()
dog.addComponent(new GLTFShape('BlockDog.glb'))
dog.addComponent(dogAnimator)

const sit = dog.getComponent(Animator).getClip('Sitting')
const drinking = dog.getComponent(Animator).getClip('Drinking')
const idle = dog.getComponent(Animator).getClip('Idle')
const walking = dog.getComponent(Animator).getClip('Walking')

sit.looping = false
idle.play()

dog.addComponent(
  new Transform({
    position: new Vector3(8, 0, 4)
  })
)

engine.addEntity(dog)

// Shark
const shark = new Entity()
const sharkAnimator = new Animator()
shark.addComponent(new GLTFShape('shark.gltf'))
shark.addComponent(sharkAnimator)

const swim = new AnimationState('swim', { layer: 0 })
const bite = new AnimationState('bite', { layer: 1 })

sharkAnimator.addClip(swim)
sharkAnimator.addClip(bite)

shark.addComponent(
  new Transform({
    position: new Vector3(8, 3, 10)
  })
)

engine.addEntity(shark)

// Dog Buttons

createButton(3, 'Stop all animations', () => {
  dogAnimator.stop()
})

createButton(3.5, 'Play Sit', () => {
  sit.play()
})

createButton(4, 'Play Idle', () => {
  idle.play()
})

createButton(4.5, 'Play Drinking', () => {
  drinking.play()
})

createButton(5, 'Play Walking', () => {
  walking.play()
})

// Shark Buttons
createButton(12, 'Toggle swim', () => {
  swim.playing = !swim.playing
})

createButton(12.5, 'Toggle bite', () => {
  bite.playing = !bite.playing
})