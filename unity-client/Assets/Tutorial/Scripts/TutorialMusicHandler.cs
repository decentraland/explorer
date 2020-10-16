using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMusicHandler : MonoBehaviour
{
    [SerializeField] AudioEvent audioEventMusic;

    private void Start()
    {
        CommonScriptableObjects.rendererState.OnChange += OnRenderStateChange;
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.rendererState.OnChange -= OnRenderStateChange;
    }

    void OnRenderStateChange(bool current, bool previous)
    {
        if (current)
            audioEventMusic.PlayScheduled(1.5f);
        else
            audioEventMusic.FadeOut(2.5f);
    }

    
}
