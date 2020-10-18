using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMusicHandler : MonoBehaviour
{
    [SerializeField] DCL.Tutorial.TutorialController tutorialController;
    [SerializeField] AudioEvent tutorialMusic, avatarEditorMusic;

    bool rendererIsReady = false, tutorialHasBeenEnabled = false;

    Coroutine fadeOut;

    private void Start()
    {
        tutorialController.OnTutorialEnabled += OnTutorialEnabled;
        tutorialController.OnTutorialDisabled += OnTutorialDisabled;
        CommonScriptableObjects.rendererState.OnChange += OnRendererStateChange;
        avatarEditorMusic.OnPlay += OnAvatarEditorMusicPlay;
        avatarEditorMusic.OnStop += OnAvatarEditorMusicStop;
    }

    private void OnDestroy()
    {
        tutorialController.OnTutorialEnabled -= OnTutorialEnabled;
        tutorialController.OnTutorialDisabled -= OnTutorialDisabled;
        CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChange;
        avatarEditorMusic.OnPlay -= OnAvatarEditorMusicPlay;
        avatarEditorMusic.OnStop -= OnAvatarEditorMusicStop;
    }

    void OnRendererStateChange(bool current, bool previous)
    {
        rendererIsReady = current;
        TryPlayingMusic();
    }

    void OnTutorialEnabled()
    {
        tutorialHasBeenEnabled = true;
        TryPlayingMusic();
    }

    void TryPlayingMusic()
    {
        if (rendererIsReady && tutorialHasBeenEnabled && !tutorialMusic.source.isPlaying)
        {
            if (fadeOut != null)
                StopCoroutine(fadeOut);
            tutorialMusic.PlayScheduled(1.5f);
        }
    }

    void OnTutorialDisabled()
    {
        if (tutorialMusic.source.isPlaying)
            fadeOut = StartCoroutine(tutorialMusic.FadeOut(3f));
        tutorialHasBeenEnabled = false;
    }

    void OnAvatarEditorMusicPlay()
    {
        if (tutorialMusic.source.isPlaying)
            fadeOut = StartCoroutine(tutorialMusic.FadeOut(1.5f, false));
    }

    void OnAvatarEditorMusicStop()
    {
        if (tutorialMusic.source.isPlaying)
            StartCoroutine(tutorialMusic.FadeIn(2.5f));
    }
}
