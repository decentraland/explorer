import { OrderedRingBuffer } from 'atomicHelpers/OrderedRingBuffer'
import { expect } from 'chai'

describe('OrderedRingBuffer', () => {
  let buffer: OrderedRingBuffer<Float32Array>
  beforeEach(() => {
    buffer = new OrderedRingBuffer(20, Float32Array)
  })

  it('can write and read simple operations', () => {
    buffer.write(Float32Array.of(10, 20, 30), 0)

    expect(buffer.read()).to.eql(Float32Array.of(10, 20, 30))

    buffer.write(Float32Array.of(40, 50, 60), 1)
    expect(buffer.read()).to.eql(Float32Array.of(40, 50, 60))
  })

  it('can write multiple and read once', () => {
    buffer.write(Float32Array.of(10, 20, 30), 0)
    buffer.write(Float32Array.of(40, 50, 60), 1)

    expect(buffer.read()).to.eql(Float32Array.of(10, 20, 30, 40, 50, 60))
  })

  it('can write when the buffer is full, overwriting the first values', () => {
    const toWrite = new Float32Array(buffer.size)

    toWrite.fill(1)

    buffer.write(toWrite, 0)
    buffer.write(Float32Array.of(10, 20, 30), 1)

    const expected = new Float32Array(toWrite)
    expected.set(Float32Array.of(10, 20, 30), toWrite.length - 3)

    expect(buffer.read()).to.eql(expected)
  })

  it('can write values bytes and still work as expected', () => {
    for (let i = 0; i < 10; i++) {
      const toWrite = new Float32Array(buffer.size)

      toWrite.fill(i)

      buffer.write(toWrite, i)
    }

    buffer.write(Float32Array.of(10, 20, 30), 11)

    const expected = new Float32Array(buffer.size)
    expected.fill(9)
    expected.set(Float32Array.of(10, 20, 30), buffer.size - 3)

    expect(buffer.read()).to.eql(expected)
  })

  it('can write a large array and it keeps the last values', () => {
    for (let i = 0; i < 10; i++) {
      const toWrite = new Float32Array(buffer.size)

      toWrite.fill(i)

      buffer.write(toWrite, i)
    }

    buffer.write(Float32Array.of(10, 20, 30), 11)

    const expected = new Float32Array(buffer.size)
    expected.fill(9)
    expected.set(Float32Array.of(10, 20, 30), buffer.size - 3)

    expect(buffer.read()).to.eql(expected)
  })
})
