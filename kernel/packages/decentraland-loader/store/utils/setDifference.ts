export function setDifference(original: Record<string, boolean>, from: Record<string, boolean>, keys?: string[]) {
  if (!keys) {
    keys = Object.keys(original)
  }
  const diff: string[] = []
  for (let key of keys) {
    if (original[key] && !from[key]) {
      diff.push(key)
    }
  }
  return diff
}
