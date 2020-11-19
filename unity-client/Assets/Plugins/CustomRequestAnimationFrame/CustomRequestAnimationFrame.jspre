
(function () {
  if (window.requestAnimationFrame && document) {
    window.capFPS = false;

    window.backgroundFPS = 30;
    window.__CURRENT_RAFS = {};

    const FPS_CAP = 40; // float precision when comparing times affects calculations, "target fps - 10" -> 40 -> 30FPS
    const FRAME_MS = 1000 / FPS_CAP;
    var callbacks = [];
    window.__requestAnimationFrame = window.__requestAnimationFrame || window.requestAnimationFrame;
    const originalRaf = window.__requestAnimationFrame || window.requestAnimationFrame;
    var prevTime = 0;

    function backgroundRAF(rafCallback) {
      setTimeout(function(){
        rafCallback(performance.now())
      }, 1000 / window.backgroundFPS);
    }

    // called every frame
    function tick(time) {
      if (!window.capFPS || time - prevTime > FRAME_MS) {
        var oldCallbacks = callbacks;
        callbacks = [];

        for (var i = 0; i < oldCallbacks.length; i++) {
            oldCallbacks[i](time);
        }

        oldCallbacks.length = 0;
        prevTime = time;
      }

      scheduleNext();
    }

    function scheduleNext() {
      if (document.hidden) {
        backgroundRAF(tick);
      } else {
        // originalRaf(tick);

        var rafId = originalRaf(function(stamp) {
          tick(stamp);
          delete __CURRENT_RAFS[rafId];
        });
        __CURRENT_RAFS[rafId] = tick;
      }
    }

    window.requestAnimationFrame = function (cb) {
      return callbacks.push(cb);
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

    scheduleNext();
  }
})();
