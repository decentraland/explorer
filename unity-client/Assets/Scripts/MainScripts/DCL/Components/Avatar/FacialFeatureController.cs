using DCL.Helpers;
using System.Collections;
using System.Linq;
using UnityEngine;
using DCL;

public class FacialFeatureController
{
    public delegate void TexturesFetched(Texture texture, Texture mask);

    private string bodyShapeType;
    WearableItem wearable;

    Texture mainTexture = null;
    Texture maskTexture = null;
    bool mainTextureRetrieved = false;
    bool maskTextureRetrieved = false;
    AssetPromise_Texture mainTexturePromise = null;
    AssetPromise_Texture maskTexturePromise = null;
    TexturesFetched onTextureFetchedCallback;

    public FacialFeatureController(WearableItem wearableItem, string bodyShapeType)
    {
        this.wearable = wearableItem;
        this.bodyShapeType = bodyShapeType;
        mainTextureRetrieved = false;
        maskTextureRetrieved = false;
    }

    public IEnumerator FetchTextures(TexturesFetched onTextureFetched)
    {
        onTextureFetchedCallback = onTextureFetched;

        if (mainTextureRetrieved && maskTextureRetrieved)
        {
            onTextureFetchedCallback?.Invoke(mainTexture, maskTexture);
            yield break;
        }

        var representation = wearable.GetRepresentation(bodyShapeType);

        string mainTextureName = representation.contents.FirstOrDefault(x => !x.file.ToLower().Contains("_mask.png"))?.hash;
        string maskName = representation.contents.FirstOrDefault(x => x.file.ToLower().Contains("_mask.png"))?.hash;

        if (!string.IsNullOrEmpty(mainTextureName))
        {
            if (mainTexturePromise != null)
                AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);

            mainTexturePromise = new AssetPromise_Texture(wearable.baseUrl + mainTextureName);
            mainTexturePromise.OnSuccessEvent += (x) => { mainTexture = x.texture; mainTextureRetrieved = true; TextureFetchFinished(); };
            mainTexturePromise.OnFailEvent += (x) => { mainTexture = null; mainTextureRetrieved = true; };
            AssetPromiseKeeper_Texture.i.Keep(mainTexturePromise);
        }
        else
        {
            mainTextureRetrieved = true;
        }

        if (!string.IsNullOrEmpty(maskName))
        {
            if (maskTexturePromise != null)
                AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);

            maskTexturePromise = new AssetPromise_Texture(wearable.baseUrl + maskName);
            maskTexturePromise.OnSuccessEvent += (x) => { maskTexture = x.texture; maskTextureRetrieved = true; TextureFetchFinished(); };
            maskTexturePromise.OnFailEvent += (x) => { maskTexture = null; maskTextureRetrieved = true; };
            AssetPromiseKeeper_Texture.i.Keep(maskTexturePromise);
        }
        else
        {
            maskTextureRetrieved = true;
        }

        TextureFetchFinished();
    }

    void TextureFetchFinished()
    {
        if (mainTextureRetrieved && maskTextureRetrieved)
            onTextureFetchedCallback?.Invoke(mainTexture, maskTexture);
    }

    void OnDestroy()
    {
        if (mainTexturePromise != null)
            AssetPromiseKeeper_Texture.i.Forget(mainTexturePromise);

        if (maskTexturePromise != null)
            AssetPromiseKeeper_Texture.i.Forget(maskTexturePromise);
    }
}
