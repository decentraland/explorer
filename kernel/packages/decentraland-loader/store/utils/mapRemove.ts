export function mapRemove(map: Record<string, boolean>, items: string[]) {
  const result = { ...map }
  for (let item of items) {
    delete result[item]
  }
  return result
}
