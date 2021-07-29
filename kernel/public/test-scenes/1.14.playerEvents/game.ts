import { engine, Entity, Transform, Vector3, onPlayerExpressionObservable, TextShape } from 'decentraland-ecs'

//Create entity and assign shape
const text = new Entity()
const shape = new TextShape('Make an expression!')
text.addComponent(shape)

text.addComponent(
  new Transform({
    position: new Vector3(8, 1, 8),
    scale: new Vector3(-0.25, 0.25, -0.25)
  })
)

engine.addEntity(text)

onPlayerExpressionObservable.add(({ expressionId }) => {
  shape.value = 'Expression: ' + expressionId
})
