using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreHUDAudioHandler : MonoBehaviour
{
    [SerializeField]
    GotoMagicButton magicButton;

    AudioEventOld eventMagicPointerEnter, eventMagicPointerExit, eventMagicButtonPressed;

    float magicPointerEnterLastPlayed = 0f;

    private void Start()
    {
        AudioContainerOld ac = GetComponent<AudioContainerOld>();
        eventMagicPointerEnter = ac.GetEvent("MagicButtonEnter");
        eventMagicPointerExit = ac.GetEvent("MagicButtonExit");
        eventMagicButtonPressed = ac.GetEvent("MagicButtonPressed");

        magicButton.OnGotoMagicPointerEnter += OnMagicButtonEnter;
        magicButton.onGotoMagicPointerExit += OnMagicButtonExit;
        magicButton.OnGotoMagicPressed += OnMagicButtonPressed;
    }

    void OnMagicButtonEnter()
    {
        magicPointerEnterLastPlayed = Time.fixedTime;
        eventMagicPointerEnter.Play(true);
    }

    void OnMagicButtonExit()
    {
        if (magicPointerEnterLastPlayed < Time.fixedTime -0.3f)
            eventMagicPointerExit.Play(true);
    }

    void OnMagicButtonPressed()
    {
        eventMagicButtonPressed.Play();
    }
}
