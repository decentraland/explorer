using UnityEngine;

[CreateAssetMenu(fileName = "AudioEventWithPitchIncrement", menuName = "AudioEvents/AudioEvent - With pitch increment")]
public class AudioEvent_WithPitchIncrement : AudioEvent
{
    [SerializeField]
    float pitchIncrement;

    public override void Play(bool oneShot = false)
    {
        base.Play(oneShot);
        SetPitch(1f + pitchIncrement);
    }
}