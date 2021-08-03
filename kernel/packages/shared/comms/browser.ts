export const recommendedBrowsers: string[] = [
  "Chrome",
  "Firefox"
]

export function isWebGLCompatible() {
  // Create canvas element. The canvas is not added to the document itself, so it is never displayed in the browser window.
  var canvas = <HTMLCanvasElement>document.createElement("canvas");
  var gl = canvas.getContext("webgl2")
  return gl && gl instanceof WebGL2RenderingContext
}

export function isRecommendedBrowser() {
  for (let i = 0; i < recommendedBrowsers.length; i++) {
    if (navigator.userAgent.indexOf(recommendedBrowsers[i]) != -1) {
      return true
    }
  }

  return false
}
