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
            Debug.Log("Mordi -> tutorialMusic.source.loop = " + tutorialMusic.source.loop);
        }
    }

    void OnRendererStateChange(bool current, bool previous)
    {
        Debug.Log("Mordi -> " + name + " - OnRendererStateChange()");
        rendererIsReady = current;
        TryPlayingMusic();
    }

    void OnTutorialEnabled()
    {
        Debug.Log("Mordi -> " + name + " - OnTutorialEnabled()");
        tutorialHasBeenEnabled = true;
        TryPlayingMusic();
    }

    void TryPlayingMusic()
    {
        Debug.Log("Mordi -> " + name + " - TryPlayingMusic()");
        if (rendererIsReady && tutorialHasBeenEnabled && !tutorialMusic.source.isPlaying)
        {
            if (fadeOut != null)
            {
                Debug.Log("Mordi -> :: StopCoroutine(fadeOut)");
                StopCoroutine(fadeOut);
            }
            Debug.Log("Mordi -> " + name + ":: tutorialMusic.PlayScheduled(1.5f)");
            tutorialMusic.PlayScheduled(1.5f);
        }
    }

    void OnTutorialDisabled()
    {
        Debug.Log("Mordi -> " + name + " - OnTutorialDisabled()");
        if (tutorialMusic.source.isPlaying)
            fadeOut = StartCoroutine(tutorialMusic.FadeOut(3f));
        tutorialHasBeenEnabled = false;
    }

    void OnAvatarEditorMusicPlay()
    {
        Debug.Log("Mordi -> " + name + " - OnAvatarEditorMusicPlay()");
        if (tutorialMusic.source.isPlaying)
            fadeOut = StartCoroutine(tutorialMusic.FadeOut(1.5f, false));
    }

    void OnAvatarEditorMusicStop()
    {
        Debug.Log("Mordi -> " + name + " - OnAvatarEditorMusicStop()");
        if (tutorialMusic.source.isPlaying)
            StartCoroutine(tutorialMusic.FadeIn(2.5f));
    }
}
