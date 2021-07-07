export function isWebGLCompatible() {
  // Create canvas element. The canvas is not added to the document itself, so it is never displayed in the browser window.
  var canvas = <HTMLCanvasElement>document.createElement("canvas");
  var gl = canvas.getContext("webgl") || canvas.getContext("experimental-webgl") || canvas.getContext("webkit-3d") || canvas.getContext("moz-webgl");
  return gl && gl instanceof WebGLRenderingContext
}