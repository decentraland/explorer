declare const globalThis: { ROOT_URL?: string }

const base =
  typeof globalThis.ROOT_URL !== 'undefined'
    ? new URL(globalThis.ROOT_URL, document.location.toString()).toString()
    : new URL('.', document.location.toString()).toString()

console.log('> Kernel base: ' + base)

export function getResourcesURL(path: string) {
  return new URL(path, base).toString()
}
