/**
 * Interface?
 */

mergeInto(LibraryManager.library, {
  StartDecentraland: function() {
    window.DCL.EngineStarted();
  },
  MessageFromEngine: function(type, message) {
    window.DCL.MessageFromEngine(Pointer_stringify(type), Pointer_stringify(message));
  },
  LockBrowserCursor: function() {
    var canvas = document.getElementsByTagName("canvas")[0];
    canvas.requestPointerLock = canvas.requestPointerLock || canvas.mozRequestPointerLock;
    canvas.requestPointerLock();
  },
    UnlockBrowserCursor: function() {
    document.exitPointerLock = document.exitPointerLock || document.mozExitPointerLock;
    document.exitPointerLock();
  }
});
