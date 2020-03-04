export function mapAdd(map: Record<string, boolean>, items: string[]) {
  return items.reduce(
    (cumm, item) => {
      cumm[item] = true
      return cumm
    },
    { ...map }
  )
}
