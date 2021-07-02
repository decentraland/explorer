declare const globalThis: { ROOT_URL?: string }

export function getResourcesURL() {
  if (typeof globalThis.ROOT_URL !== 'undefined') {
    return new URL(globalThis.ROOT_URL, document.location.toString()).toString()
  }

  let pathName = location.pathname.split('/')
  if (pathName[pathName.length - 1].includes('.')) {
    pathName.pop()
  }

  const basePath = origin + pathName.join('/')
  if (basePath.endsWith('/')) return basePath.slice(0, -1)
  return basePath
}
