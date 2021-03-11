using System;
using DCL;
using UnityEngine;

[RequireComponent(typeof(AvatarRenderer))]
public class ParticlesOnAvatarVisualCue : MonoBehaviour
{
    [SerializeField] private AvatarRenderer.VisualCue avatarVisualCue;
    [SerializeField] private GameObject particlePrefab;

    private AvatarRenderer avatarRenderer;
    private void Awake()
    {
        avatarRenderer = GetComponent<AvatarRenderer>();
        if (avatarRenderer == null)
            return;
        avatarRenderer.OnVisualCue += OnVisualCue;
    }

    private void OnVisualCue(AvatarRenderer.VisualCue cue)
    {
        if (cue == avatarVisualCue && particlePrefab != null)
        {
            Instantiate(particlePrefab).transform.position += avatarRenderer.transform.position;
        }
    }

    private void OnDestroy()
    {
        if (avatarRenderer == null)
            return;
        avatarRenderer.OnVisualCue -= OnVisualCue;
    }
}