
(function () {
  if (window.requestAnimationFrame && document) {
    window.backgroundFPS = 30;
    window.__requestAnimationFrame = window.requestAnimationFrame;
    window.__CURRENT_RAFS = {};

    window.FPSCapMethod = (location.search.indexOf('SETTIMEOUT_CAP_30') !== -1) ? "setTimeout"
                          : (location.search.indexOf('RAF_CAP_30') !== -1) ? "RAF"
                          : (location.search.indexOf('TICK_CAP_30') !== -1) ? "Tick"
                          : "none";

    function backgroundRAF(rafCallback) {
      setTimeout(function(){
        rafCallback(performance.now())
      }, 1000 / window.backgroundFPS);
    }

    switch(window.FPSCapMethod) {
      case 'setTimeout':
        // Taken from UnityInstallDirectory/PlaybackEngines/WebGLSupport/BuildTools//Emscripten/src/library_browser.js -> fakeRequestAnimationFrame(), and customized
        function fakeRAFWithSetTimeout(func) {
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

        window.requestAnimationFrame = function (unityRAFCallback) {
          if (document.hidden) {
            backgroundRAF(unityRAFCallback);
          } else {
            fakeRAFWithSetTimeout(unityRAFCallback);
          }
        };
        break;

      case 'RAF':
        window.unitySkippedLastRAF = true;
        window.cachedUnityRAFCallback;

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
        break;

      case 'Tick':
        const TARGET_FPS = 40; // ENDS UP WITH A FRAMERATE EQUAL TO THE "TARGET_FPS - 10" -> 30FPS
        var callbacks = [];
        window.__requestAnimationFrame = window.__requestAnimationFrame || window.requestAnimationFrame;
        const originalRaf = window.__requestAnimationFrame || window.requestAnimationFrame;
        const FRAME_MS = 1000 / TARGET_FPS;
        var prevTime = 0;

        function tick(time) {
          if (time - prevTime > FRAME_MS) {
            var oldCallbacks = callbacks;
            callbacks = [];
            for (var i = 0; i < oldCallbacks.length; i++) {
              try {
                oldCallbacks[i](time);
              } catch (e) {
                console.error(e);
              }
            }
            oldCallbacks.length = 0;
            prevTime = time;
          }
          scheduleNext();
        }

        function scheduleNext() {
          if (document.hidden) {
            setTimeout(function() { tick(performance.now()); }, FRAME_MS);
          } else {
            originalRaf(tick);
          }
        }

        window.requestAnimationFrame = function (cb) {
          return callbacks.push(cb);
        };

        scheduleNext();

        break;

      default: // 'none'
        window.requestAnimationFrame = function (rafCallback) {
          if (document.hidden) {
            backgroundRAF(rafCallback);
          } else {
            var rafId = __requestAnimationFrame(function(stamp) {
              rafCallback(stamp);
              delete __CURRENT_RAFS[rafId];
            });
            __CURRENT_RAFS[rafId] = rafCallback;
          }
        };
    }

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
