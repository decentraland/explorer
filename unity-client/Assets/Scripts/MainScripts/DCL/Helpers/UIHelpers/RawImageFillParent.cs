using UnityEngine;
using UnityEngine.UI;

public class RawImageFillParent : RawImage
{
    new public Texture texture
    {
        set
        {
            base.texture = value;

            if (value != null)
            {
                ResizeFillParent();
            }
        }
        get
        {
            return base.texture;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        if (texture != null)
        {
            ResizeFillParent();
        }
    }

    void ResizeFillParent()
    {
        RectTransform parent = transform.parent as RectTransform;

        float widthDiff = parent.rect.width - base.texture.width;
        float heightDiff = parent.rect.height - base.texture.height;

        float h, w;
        if (heightDiff > widthDiff)
        {
            h = parent.rect.height;
            w = h * (base.texture.width / (float)base.texture.height);
        }
        else
        {
            w = parent.rect.width;
            h = w * (base.texture.height / (float)base.texture.width);
        }

        rectTransform.sizeDelta = new Vector2(w, h);
    }
}
