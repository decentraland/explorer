using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDAudioPlayer : MonoBehaviour
{
    public static HUDAudioPlayer i { get; private set; }

    [HideInInspector]
    public AudioContainer audioContainer;

    private void Awake()
    {
        audioContainer = GetComponent<AudioContainer>();
        i = this;
    }
}
