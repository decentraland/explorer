export function setRemove(from: string[], remove: string[]) {
  const toRemove: Record<string, boolean> = {}
  for (let item of remove) {
    toRemove[item] = true
  }
  const result: string[] = []
  for (let item of from) {
    if (!toRemove[item]) {
      result.push(item)
    }
  }
  return result
}
