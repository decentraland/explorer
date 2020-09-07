using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRendererAudioHandler : MonoBehaviour
{
    AudioEventOld eventMapParcelHighlight;

    private void Awake()
    {
        AudioContainerOld ac = GetComponent<AudioContainerOld>();
        eventMapParcelHighlight = ac.GetEvent("MapParcelHighlight");
        eventMapParcelHighlight.SetPitch(4f);
    }

    public void PlayMapParcelHighlight()
    {
        eventMapParcelHighlight.Play(true);
    }
}
