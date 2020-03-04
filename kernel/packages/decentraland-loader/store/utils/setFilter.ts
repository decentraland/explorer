export function setFilter(from: string[], ...filters: Record<string, any>[]) {
  const result = []
  for (let item of from) {
    let add = true
    for (let filter of filters) {
      if (filter[item]) {
        add = false
        break
      }
    }
    if (add) {
      result.push(item)
    }
  }
  return result
}
