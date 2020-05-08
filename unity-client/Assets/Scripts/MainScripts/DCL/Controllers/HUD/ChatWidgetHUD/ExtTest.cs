using UnityEngine;

public class ExtTest : MonoBehaviour
{
    public RectTransform viewport;
    void Update()
    {
        (transform as RectTransform).CountCornersVisibleFrom(viewport);
    }
}
