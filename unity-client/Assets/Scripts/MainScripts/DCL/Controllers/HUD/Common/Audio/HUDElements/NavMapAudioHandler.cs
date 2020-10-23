using UnityEngine;

public class NavMapAudioHandler : MonoBehaviour
{
    private void Awake()
    {
        DestroyImmediate(this);
        return;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            AudioScriptableObjects.buttonClick.Play(true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            AudioScriptableObjects.buttonRelease.Play(true);
        }
    }
}
