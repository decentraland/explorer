
(function () {
  // Taken from UnityInstallDirectory/PlaybackEngines/WebGLSupport/BuildTools//Emscripten/src/library_browser.js -> fakeRequestAnimationFrame(), and customized
  window.requestAnimationFrame = function (rafCallback) {
    // try to keep 30fps between calls to here
    var now = Date.now();
    var targetFPS = 30;
    if (Browser.nextRAF === 0) {
      Browser.nextRAF = now + (1000/targetFPS);
    } else {
      while (now + 2 >= Browser.nextRAF) { // fudge a little, to avoid timer jitter causing us to do lots of delay:0
        Browser.nextRAF += (1000/targetFPS);
      }
    }
    console.log('pravs - custom RAF, fps: ' + targetFPS)
    var delay = Math.max(Browser.nextRAF - now, 0);
    setTimeout(rafCallback, delay);
  };
})();
