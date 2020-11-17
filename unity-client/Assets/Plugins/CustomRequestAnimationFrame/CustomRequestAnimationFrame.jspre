
(function () {
  if (window.requestAnimationFrame && document) {
    window.setTargetFPS = function (targetFPS) {
      const TARGET_FPS = (targetFPS > 0) ? (targetFPS + 10) : 70; // ENDS UP WITH A FRAMERATE EQUAL TO THE "target fps - 10" -> 70 -> 60FPS

      window.targetFPSMS = 1000 / TARGET_FPS;
    };
    window.setTargetFPS(-1);

    var callbacks = [];
    window.__requestAnimationFrame = window.__requestAnimationFrame || window.requestAnimationFrame;
    const originalRaf = window.__requestAnimationFrame || window.requestAnimationFrame;
    var prevTime = 0;

    function tick(time) {
      if (time - prevTime > window.targetFPSMS) {
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
        setTimeout(function() { tick(performance.now()); }, window.targetFPSMS);
      } else {
        originalRaf(tick);
      }
    }

    window.requestAnimationFrame = function (cb) {
      return callbacks.push(cb);
    };

    scheduleNext();
  }
})();
