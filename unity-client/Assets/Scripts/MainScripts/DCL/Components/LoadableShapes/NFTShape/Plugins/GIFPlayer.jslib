var GIFPlayer = {
  $frames: {},

  GIFPlayerCreate: function () {
    // var img = new Image();
    // img.crossOrigin = "";
    // img.src = "https://i.imgur.com/ZKMnXce.png";
  },

  SetTexturePointer: function (texturePtr, imgSource, isWebGL1) {
    imgSource = Pointer_stringify(imgSource);

    console.log("pravs - SetTexturePointer (emscripten) - window.DCL.InitializeGIF()")
    console.log("pravs - SetTexturePointer (emscripten) - imgsource: " + imgSource)
    console.log("pravs - SetTexturePointer (emscripten) - texturePtr: " + texturePtr)
    console.log("pravs - SetTexturePointer (emscripten) - isWebGL1: " + isWebGL1)

    window.DCL.InitializeGIF(imgSource, function (image) {
      console.log("pravs - SetTexturePointer (emscripten) - CALLBACK CALLED!!!")

      GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[texturePtr]);

      if (isWebGL1) {
        GLctx.texImage2D(
          GLctx.TEXTURE_2D,
          0,
          GLctx.RGBA,
          GLctx.RGBA,
          GLctx.UNSIGNED_BYTE,
          image
        );
      } else {
        GLctx.texSubImage2D(
          GLctx.TEXTURE_2D,
          0,
          0,
          0,
          GLctx.RGBA,
          GLctx.UNSIGNED_BYTE,
          image
        );
      }
    });
  }
};
autoAddDeps(GIFPlayer, "$frames");
mergeInto(LibraryManager.library, GIFPlayer);
