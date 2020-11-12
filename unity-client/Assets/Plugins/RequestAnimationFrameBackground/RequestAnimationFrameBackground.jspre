
(function () {
  if (window.requestAnimationFrame && document) {
    window.backgroundFPS = 30;
    window.__requestAnimationFrame = window.requestAnimationFrame;
    window.__CURRENT_RAFS = {};

    function backgroundRAF(rafCallback) {
      setTimeout(function(){
        rafCallback(performance.now())
      }, 1000 / window.backgroundFPS);
    }

    // Taken from UnityInstallDirectory/PlaybackEngines/WebGLSupport/BuildTools//Emscripten/src/library_browser.js -> fakeRequestAnimationFrame(), and customized
    function fakeRequestAnimationFrame(func) {
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

      var delay = Math.max(Browser.nextRAF - now, 0);
      setTimeout(func, delay);
    }

    window.requestAnimationFrame = function (rafCallback) {
      if (document.hidden) {
        backgroundRAF(rafCallback);
      } else {
        fakeRequestAnimationFrame(rafCallback)
        // var rafId = __requestAnimationFrame(function(stamp) {
        //   rafCallback(stamp);
        //   delete __CURRENT_RAFS[rafId];
        // });
        // __CURRENT_RAFS[rafId] = rafCallback;
      }
    };

    function switchToBackground() {
      Object.keys(__CURRENT_RAFS).forEach(function(rafId) {
        window.cancelAnimationFrame(rafId);
        rafCallback = __CURRENT_RAFS[rafId];
        delete __CURRENT_RAFS[rafId];
        backgroundRAF(rafCallback);
      });
    }

    document.addEventListener("visibilitychange", function() {
      if(document.hidden) {
        switchToBackground();
      }
    })
  }
})();
