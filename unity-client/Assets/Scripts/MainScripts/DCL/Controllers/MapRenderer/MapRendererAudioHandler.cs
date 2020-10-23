using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRendererAudioHandler : MonoBehaviour
{
    [SerializeField]
    DCL.MapRenderer mapRenderer;

    [SerializeField]
    AudioEvent eventMapParcelHighlight;

    private void Awake()
    {
        DestroyImmediate(this);
        return;

        mapRenderer.OnMovedParcelCursor += OnMovedParcelCursor;
    }

    public void OnMovedParcelCursor()
    {
        eventMapParcelHighlight.Play(true);
    }
}
