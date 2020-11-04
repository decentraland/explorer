import { expect } from 'chai'
import { Entity, BoxShape, Transform, Vector3, engine } from 'decentraland-ecs/src';

describe('Engine tests', () => {
  it('getComponentGroupReturnsCachedGroup', () => {

    let box = new Entity()
    box.addComponent(new BoxShape())
    box.addComponent(
      new Transform({
        position: new Vector3(8, 0, 8),
      })
    )
    const firstComponentGroup = engine.getComponentGroup(BoxShape, Transform)
    const secondComponentGroup = engine.getComponentGroup(BoxShape, Transform)
    const thirdComponentGroup = engine.getComponentGroup(BoxShape, Transform)

    expect(firstComponentGroup).to.same(
      secondComponentGroup,
      'returnCachedComponentGroup01'
    )

    expect(secondComponentGroup).to.same(
      thirdComponentGroup,
      'returnCachedComponentGroup02'
    )
  })
})
