
(function () {
  if (window.requestAnimationFrame && document) {
    window.backgroundFPS = 30;
    window.__requestAnimationFrame = window.requestAnimationFrame;
    window.__CURRENT_RAFS = {};

    window.unitySkippedLastRAF = true;
    window.cachedUnityRAFCallback;

    function backgroundRAF(rafCallback) {
      setTimeout(function(){
        rafCallback(performance.now())
      }, 1000 / window.backgroundFPS);
    }

    function nonUnityRAFCallback(stamp) {
      window.requestAnimationFrame(nonUnityRAFCallback); // ends up calling the normal RAF again
    }

    window.requestAnimationFrame = function (unityRAFCallback) { // Unity's rafCallback calls RAF inside
      if (document.hidden) {
        backgroundRAF(unityRAFCallback);
      } else {
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
