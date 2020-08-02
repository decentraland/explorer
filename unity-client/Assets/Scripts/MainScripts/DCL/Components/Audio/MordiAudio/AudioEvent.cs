using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReorderableList;

[System.Serializable]
public class AudioEvent
{
    [System.Serializable]
    public class AudioClipList : ReorderableArray<AudioClip>
    {
    }

    // Index for clips-array
    private int index;

    public string name;
    public bool loop = false;
    [Range(0f, 1f)]
    public float volume = 1.0f;
    [Range(0f, 1f)]
    public float randomPitch = 0.0f;
    public bool playOnAwake = false;
    [Reorderable]
    public AudioClipList clips;

    [HideInInspector]
    public AudioSource source;

    private float pitch = 1f;

    public void Initialize()
    {
        RandomizeIndex();
    }

    public void RandomizeIndex()
    {
        int newIndex;
        do
        {
            newIndex = Random.Range(0, clips.Length);
        } while (clips.Length > 1 && newIndex == index);
        index = newIndex;
    }

    public void Play(bool oneShot = false)
    {
        // Check if AudioSource is active
        if (!source.gameObject.activeSelf)
        {
            return;
        }

        // Set clip
        source.clip = clips[index];

        // Set pitch
        source.pitch = pitch + Random.Range(0f, randomPitch) - (randomPitch * 0.5f);

        // Play from source
        if (oneShot)
        {
            source.PlayOneShot(source.clip);
        }
        else
        {
            source.Play();
        }

        RandomizeIndex();
    }

    public void Stop()
    {
        source.Stop();
    }

    public void SetIndex(int index)
    {
        this.index = index;
    }

    public void SetPitch(float pitch)
    {
        this.pitch = pitch;
    }
}