/**
 * Interface?
 */

mergeInto(LibraryManager.library, {
  StartDecentraland: function() {
    window.DCL.EngineStarted();
  },
  MessageFromEngine: function(type, message) {
    window.DCL.MessageFromEngine(Pointer_stringify(type), Pointer_stringify(message));
  }
});
