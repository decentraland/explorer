declare const globalThis: { KERNEL_ROOT?: string }

const base =
  typeof globalThis.KERNEL_ROOT !== 'undefined'
    ? new URL(globalThis.KERNEL_ROOT, document.location.toString()).toString()
    : new URL('.', document.location.toString()).toString()

export function getResourcesURL(path: string) {
  return new URL(path, base).toString()
}
