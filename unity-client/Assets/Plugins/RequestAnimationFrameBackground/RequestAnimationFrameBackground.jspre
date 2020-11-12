
(function () {
  if (window.requestAnimationFrame && document) {
    window.backgroundFPS = 30;
    window.__requestAnimationFrame = window.requestAnimationFrame;
    window.__CURRENT_RAFS = {};

    window.skippedLastRAF = true;
    window.cachedCallback;

    function backgroundRAF(rafCallback) {
      setTimeout(function(){
        rafCallback(performance.now())
      }, 1000 / window.backgroundFPS);
    }

    function emptyRAF(stamp) {
      window.requestAnimationFrame(emptyRAF); // just calls RAF again for the whole thing to be evaluated again
    }

    window.requestAnimationFrame = function (rafCallback) { // the rafCallback calls RAF inside
      if (document.hidden) {
        backgroundRAF(rafCallback);
      } else {
        var rafId = __requestAnimationFrame(function(stamp) {
          if (window.skippedLastRAF) {
            window.skippedLastRAF = false;

            if (window.cachedCallback) {
              window.cachedCallback(stamp);

              window.cachedCallback = rafCallback;
            } else {
              rafCallback(stamp);
            }
          } else {
            window.skippedLastRAF = true;

            window.cachedCallback = rafCallback;

            window.requestAnimationFrame(emptyRAF); // skipping unity's rafCallback
          }

          delete __CURRENT_RAFS[rafId];
        });
        __CURRENT_RAFS[rafId] = rafCallback;
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
