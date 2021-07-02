export const compatibleBrowsers: string[] = [
  "Chrome",
  "Firefox"
]

export function isCompatibleBrowser() {
  for (let i = 0; i < compatibleBrowsers.length; i++) {
    if (navigator.userAgent.indexOf(compatibleBrowsers[i]) != -1) {
      return true
    }
  }

  return false
}
