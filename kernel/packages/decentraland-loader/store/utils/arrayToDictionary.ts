export function arrayToDictionary(array: string[]): Record<string, boolean> {
  const result: Record<string, boolean> = {}
  for (let i of array) {
    result[i] = true
  }
  return result
}
