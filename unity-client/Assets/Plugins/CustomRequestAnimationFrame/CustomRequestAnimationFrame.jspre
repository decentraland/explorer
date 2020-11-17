
(function () {
  if (window.requestAnimationFrame && document) {
    window.capFPS = false;

    const FPS_CAP = 40; // float precision when comparing times affects calculations, "target fps - 10" -> 40 -> 30FPS
    const FRAME_MS = 1000 / FPS_CAP;
    var callbacks = [];
    window.__requestAnimationFrame = window.__requestAnimationFrame || window.requestAnimationFrame;
    const originalRaf = window.__requestAnimationFrame || window.requestAnimationFrame;
    var prevTime = 0;

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
        setTimeout(function() { tick(performance.now()); }, FRAME_MS);
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
