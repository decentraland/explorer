export class SortedLimitedQueue<T> {
  private internalArray: T[]

  constructor(private readonly maxLength: number, private readonly sortCriteria: (a: T, b: T) => number) {
    this.internalArray = []
  }

  queue(item: T) {
    let insertIndex = 0

    // Since the most likely scenario for our use case is that we insert the item at the end,
    // we start by the end. This may be parameterized in the future
    for (let i = this.internalArray.length - 1; i >= 0; i--) {
      if (this.sortCriteria(item, this.internalArray[i]) > 0) {
        insertIndex = i + 1
        break
      }
    }

    this.internalArray.splice(insertIndex, 0, item)

    if (this.internalArray.length > this.maxLength) {
      this.internalArray.shift()
    }
  }

  dequeue(): T | undefined {
    return this.internalArray.shift()
  }

  dequeueItems(count: number): T[] {
    return this.internalArray.splice(0, count)
  }
}
