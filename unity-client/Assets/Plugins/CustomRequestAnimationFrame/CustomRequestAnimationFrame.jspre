
(function () {
  if (window.requestAnimationFrame && document) {
    window.backgroundFPS = 30;
    window.__requestAnimationFrame = window.requestAnimationFrame;
    window.__CURRENT_RAFS = {};

    window.unitySkippedLastRAF = true;
    window.cachedUnityRAFCallback;
    window.useRAF = true;

    function backgroundRAF(rafCallback) {
      setTimeout(function(){
        rafCallback(performance.now())
      }, 1000 / window.backgroundFPS);
    }

    function nonUnityRAFCallback(stamp) {
      window.requestAnimationFrame(nonUnityRAFCallback); // ends up calling the normal RAF again
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

    window.requestAnimationFrame = function (unityRAFCallback) { // Unity's rafCallback calls RAF inside
      if (document.hidden) {
        backgroundRAF(unityRAFCallback);
      } else {
        window.useRAF = location.search.indexOf('NO_RAF') === -1; // for testing purposes

        if (window.useRAF) {
          var rafId = __requestAnimationFrame(function(stamp) {
            if (window.unitySkippedLastRAF) {
              window.unitySkippedLastRAF = false;

              if (window.cachedUnityRAFCallback) {
                // use cached callback
                window.cachedUnityRAFCallback(stamp);

                // save new callback for next complete RAF
                window.cachedUnityRAFCallback = unityRAFCallback;
              } else {
                unityRAFCallback(stamp);
              }
            } else {
              window.unitySkippedLastRAF = true;

              // save callback for next complete RAF
              window.cachedUnityRAFCallback = unityRAFCallback;

              // skip unity's rafCallback
              window.requestAnimationFrame(nonUnityRAFCallback);
            }

            delete __CURRENT_RAFS[rafId];
          });
          __CURRENT_RAFS[rafId] = unityRAFCallback;
        } else {
          fakeRequestAnimationFrame(unityRAFCallback)
        }
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
