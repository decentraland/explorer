using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildModeInWorldLoadingAudioHandler : MonoBehaviour
{
    [SerializeField]
    BuilderInWorldLoadingView loadingView;

    private void Start() {
        loadingView.OnHide += OnLoadingViewHide;
    }

    private void OnDestroy() {
        loadingView.OnHide -= OnLoadingViewHide;
    }

    private void OnEnable() {
        AudioScriptableObjects.builderEnter.Play();
    }

    private void OnLoadingViewHide() {
        AudioScriptableObjects.builderReady.Play();
    }
}
