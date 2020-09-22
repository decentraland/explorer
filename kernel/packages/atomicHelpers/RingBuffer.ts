type TypedArray =
  | Int8Array
  | Uint8Array
  | Uint8ClampedArray
  | Int16Array
  | Uint16Array
  | Int32Array
  | Uint32Array
  | Float32Array
  | Float64Array

type Chunk = {
  order: number
  startPointer: number
  length: number
}  

export class OrderedRingBuffer<T extends TypedArray> {
  private writePointer: number = 0
  private readPointer: number = 0
  private buffer: T

  private chunks: Chunk[] = [] 

  constructor(public readonly size: number, private readonly ArrayTypeConstructor: { new (size: number): T }) {
    this.buffer = new this.ArrayTypeConstructor(size)
  }

  readAvailableCount() {
    return this.writePointer - this.readPointer
  }

  write(array: T, order: number, length?: number) {
    /*
     * - Determinar si es el último chunk. Si es el último, la write position se mantiene y todo se hace de la misma forma.
     * - Si no es el último, tomar como write position la posición del chunk inmediatamente posterior. 
     * - - Apendear los chunks que se van a sobreescribir al final
     */
    const len = length || array.length

    let toWrite = array
    if (len > this.buffer.length) {
      // If too many bytes are provided, we only write the last ones.
      toWrite = array.slice(array.length - this.buffer.length, array.length) as T
    }

    const writePosition = this.writePointer % this.buffer.length

    const endIndex = writePosition + len

    if (endIndex > this.buffer.length) {
      const partitionIndex = this.buffer.length - writePosition
      this.buffer.set(toWrite.slice(0, partitionIndex), writePosition)
      this.buffer.set(toWrite.slice(partitionIndex, len), 0)
    } else {
      this.buffer.set(toWrite.slice(0, len), writePosition)
    }

    this.writePointer += len

    const minReadPointer = this.writePointer - this.buffer.length

    if (this.readPointer < minReadPointer) {
      this.readPointer = minReadPointer
    }
  }

  read(readCount?: number): T {
    const maxCountToRead = this.writePointer - this.readPointer

    const count = readCount ? Math.min(readCount, maxCountToRead) : maxCountToRead

    const readPosition = this.readPointer % this.buffer.length

    const endIndex = readPosition + count

    let result: T

    if (endIndex > this.buffer.length) {
      result = new this.ArrayTypeConstructor(count)
      result.set(this.buffer.slice(readPosition, this.buffer.length))
      result.set(this.buffer.slice(0, endIndex - this.buffer.length), this.buffer.length - readPosition)
    } else {
      result = this.buffer.slice(readPosition, endIndex) as T
    }

    this.readPointer += count

    return result
  }
}
