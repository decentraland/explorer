export function unique(...arrays: string[][]) {
  const seen: Record<string, boolean> = {}
  const result: string[] = []
  for (let array of arrays) {
    for (let item of array) {
      if (!seen[item]) {
        result.push(item)
        seen[item] = true
      }
    }
  }
  return result
}
