using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMusicHandler : MonoBehaviour
{
    [SerializeField] AudioEvent audioEventMusic, avatarEditorMusic;

    private void Start()
    {
        CommonScriptableObjects.rendererState.OnChange += OnRenderStateChange;
        avatarEditorMusic.OnPlay += OnOtherMusicPlay;
        avatarEditorMusic.OnStop += OnOtherMusicStop;
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.rendererState.OnChange -= OnRenderStateChange;
        avatarEditorMusic.OnPlay -= OnOtherMusicPlay;
        avatarEditorMusic.OnStop -= OnOtherMusicStop;
    }

    void OnRenderStateChange(bool current, bool previous)
    {
        if (current)
            audioEventMusic.PlayScheduled(1.5f);
        else
            StartCoroutine(audioEventMusic.FadeOut(2.5f));
    }

    void OnOtherMusicPlay()
    {
        StartCoroutine(audioEventMusic.FadeOut(1.5f, false));
    }

    void OnOtherMusicStop()
    {
        StartCoroutine(audioEventMusic.FadeIn(2.5f));
    }
}
