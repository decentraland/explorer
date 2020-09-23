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
    // We find those chunks that should be after this chunk
    const nextChunks = this.chunks.filter((it) => it.order > order)

    if (nextChunks.length === 0) {
      // If there are no chunks that should be after this chunk, then we just need to write the chunk at the end.
      this.chunks.push({
        order,
        startPointer: this.writePointer,
        length: length || array.length
      })

      this.writeAt(array, this.writePointer, length)
    } else {
      // Otherwise, we need to get those chunks that should be after this one, and write them one after the other

      let writePointer = nextChunks[0].startPointer

      const newChunk = {
        order,
        startPointer: writePointer,
        length: length || array.length
      }

      // Chunks are ordered by "order", so we need to ensure that we place this new chunk in the corresponding index.
      this.chunks.splice(this.chunks.length - nextChunks.length, 0, newChunk)

      const arraysToWrite = [length ? array.slice(0, length) : array] as T[]

      // We get the arrays for each chunk, and we update their pointers while we are at it
      nextChunks.forEach((chunk) => {
        arraysToWrite.push(this.arrayForChunk(chunk))

        chunk.startPointer += newChunk.length
      })

      // We write starting from the position of the first chunk that will be rewritten
      arraysToWrite.forEach((toWrite) => {
        this.writeAt(toWrite, writePointer)
        writePointer += toWrite.length
      })
    }
  }

  arrayForChunk(chunk: Chunk): T {
    return this.peek(chunk.startPointer, chunk.length)
  }

  peek(startPointer?: number, readCount?: number): T {
    if (!startPointer) startPointer = this.readPointer

    const maxCountToRead = this.writePointer - this.readPointer

    const count = readCount ? Math.min(readCount, maxCountToRead) : maxCountToRead

    const readPosition = startPointer % this.buffer.length

    const endIndex = readPosition + count

    let result: T

    if (endIndex > this.buffer.length) {
      result = new this.ArrayTypeConstructor(count)
      result.set(this.buffer.slice(readPosition, this.buffer.length))
      result.set(this.buffer.slice(0, endIndex - this.buffer.length), this.buffer.length - readPosition)
    } else {
      result = this.buffer.slice(readPosition, endIndex) as T
    }

    return result
  }

  read(readCount?: number): T {
    const result = this.peek(this.readPointer, readCount)

    this.readPointer += result.length

    this.discardUnreadableChunks()

    return result
  }

  private writeAt(array: T, startPointer: number, length?: number) {
    const len = length || array.length

    let toWrite = array
    if (len > this.buffer.length) {
      // If too many bytes are provided, we only write the last ones.
      toWrite = array.slice(array.length - this.buffer.length, array.length) as T
    }

    const writePosition = startPointer % this.buffer.length

    const endIndex = writePosition + len

    if (endIndex > this.buffer.length) {
      const partitionIndex = this.buffer.length - writePosition
      this.buffer.set(toWrite.slice(0, partitionIndex), writePosition)
      this.buffer.set(toWrite.slice(partitionIndex, len), 0)
    } else {
      this.buffer.set(toWrite.slice(0, len), writePosition)
    }

    const endPointer = startPointer + len

    if (endPointer > this.writePointer) {
      this.writePointer = endPointer
    }

    this.updateReadPointerToMinReadPosition()
    this.discardUnreadableChunks()
  }

  private updateReadPointerToMinReadPosition() {
    const minReadPointer = this.writePointer - this.buffer.length

    if (this.readPointer < minReadPointer) {
      this.readPointer = minReadPointer
    }
  }

  private discardUnreadableChunks() {
    const isReadable = (chunk: Chunk) => {
      // A chunk is readable if its end pointer is ahead of the read pointer
      const endPointer = chunk.startPointer + chunk.length
      return endPointer > this.readPointer
    }

    this.chunks = this.chunks.filter(isReadable)

    if (this.chunks.length > 0 && this.chunks[0].startPointer < this.readPointer) {
      this.chunks[0].startPointer = this.readPointer
    }
  }
}
