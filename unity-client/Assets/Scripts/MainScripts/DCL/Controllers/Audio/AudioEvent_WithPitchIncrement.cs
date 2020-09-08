using UnityEngine;

[CreateAssetMenu(fileName = "AudioEventWithPitchIncrement", menuName = "AudioEvents/AudioEvent - With pitch increment")]
public class AudioEvent_WithPitchIncrement : AudioEvent
{
    [SerializeField]
    float pitchIncrement;

    public override void Initialize(AudioSource audioSource)
    {
        base.Initialize(audioSource);
        onPlay += OnPlay;
    }

    void OnPlay()
    {
        SetPitch(pitch + pitchIncrement);
    }
}