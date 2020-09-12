var Clipboard = {
  initialize: function(csCallback){
    window.nativeClipboardReadText = function (text, error){
      const bufferSize = lengthBytesUTF8(text) + 1
      const ptrText = _malloc(bufferSize)
      stringToUTF8(text, ptrText, bufferSize)
      const intError = error? 0 : 1
      Runtime.dynCall('vii', csCallback, [ptrText, intError]);
    }
  },

  writeText: function (text){
    navigator.clipboard.writeText(Pointer_stringify(text))
  },

  readText: function (){
    // NOTE: workaround cause jslib don't support async functions
    eval("navigator.clipboard.readText()" +
      ".then(text => window.nativeClipboardReadText(text, false))"+
      ".catch(e => window.nativeClipboardReadText(e.message, true))")
  }
};
mergeInto(LibraryManager.library, Clipboard);
