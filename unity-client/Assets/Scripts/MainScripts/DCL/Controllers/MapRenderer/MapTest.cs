using DCL;
using UnityEngine;

public class MapTest : MonoBehaviour
{
    public RectTransform viewport;
    MapRenderer mapR;
    public RectTransform playerImage;

    void Start()
    {
        mapR = FindObjectOfType<MapRenderer>();
        mapR.transform.SetParent(viewport.transform, true);
        mapR.transform.SetAsFirstSibling();
        mapR.atlas.viewport = viewport;
        mapR.atlas.CenterToTile(new Vector2(0, 0));

        playerImage.position = viewport.TransformPoint(viewport.rect.center);
    }
}
