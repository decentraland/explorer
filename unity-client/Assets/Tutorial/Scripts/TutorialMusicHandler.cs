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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("tutorialMusic.source.loop = " + tutorialMusic.source.loop);
        }
    }

    void OnRendererStateChange(bool current, bool previous)
    {
        Debug.Log(name + " - OnRendererStateChange()");
        rendererIsReady = current;
        TryPlayingMusic();
    }

    void OnTutorialEnabled()
    {
        Debug.Log(name + " - OnTutorialEnabled()");
        tutorialHasBeenEnabled = true;
        TryPlayingMusic();
    }

    void TryPlayingMusic()
    {
        Debug.Log(name + " - TryPlayingMusic()");
        if (rendererIsReady && tutorialHasBeenEnabled && !tutorialMusic.source.isPlaying)
        {
            if (fadeOut != null)
            {
                Debug.Log(":: StopCoroutine(fadeOut)");
                StopCoroutine(fadeOut);
            }
            Debug.Log(name + ":: tutorialMusic.PlayScheduled(1.5f)");
            tutorialMusic.PlayScheduled(1.5f);
        }
    }

    void OnTutorialDisabled()
    {
        Debug.Log(name + " - OnTutorialDisabled()");
        if (tutorialMusic.source.isPlaying)
            fadeOut = StartCoroutine(tutorialMusic.FadeOut(3f));
        tutorialHasBeenEnabled = false;
    }

    void OnAvatarEditorMusicPlay()
    {
        Debug.Log(name + " - OnAvatarEditorMusicPlay()");
        if (tutorialMusic.source.isPlaying)
            fadeOut = StartCoroutine(tutorialMusic.FadeOut(1.5f, false));
    }

    void OnAvatarEditorMusicStop()
    {
        Debug.Log(name + " - OnAvatarEditorMusicStop()");
        if (tutorialMusic.source.isPlaying)
            StartCoroutine(tutorialMusic.FadeIn(2.5f));
    }
}
