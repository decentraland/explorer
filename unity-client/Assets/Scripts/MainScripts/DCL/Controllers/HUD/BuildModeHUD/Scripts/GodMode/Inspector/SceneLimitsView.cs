using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SceneLimitsView : MonoBehaviour
{
    internal event Action OnToggleSceneLimitsInfo;

    [Header("Sprites")]
    [SerializeField] internal Sprite openMenuSprite;
    [SerializeField] internal Sprite closeMenuSprite;

    [Header("Design")]
    [SerializeField] internal Color lowFillColor;
    [SerializeField] internal Color mediumFillColor;
    [SerializeField] internal Color highFillColor;

    [Header("Scene references")]

    [SerializeField] internal Image detailsToggleBtn;
    [SerializeField] internal GameObject sceneLimitsBodyGO;
    [SerializeField] internal TextMeshProUGUI titleTxt;
    [SerializeField] internal TextMeshProUGUI leftDescTxt, rightDescTxt;
    [SerializeField] internal Image[] limitUsageFillsImgs;

    internal bool isBodyActived => sceneLimitsBodyGO.activeSelf;

    private UnityAction updateInfoAction;
    private const int FRAMES_BETWEEN_UPDATES = 15;

    private void Update()
    {
        if (Time.frameCount % FRAMES_BETWEEN_UPDATES == 0)
            updateInfoAction();
    }

    public void SetUpdateCallback(UnityAction call)
    {
        updateInfoAction = call;
    }

    // TODO (Santi): Called from imspector!
    public void ToggleSceneLimitsInfo()
    {
        OnToggleSceneLimitsInfo?.Invoke();
    }

    public void SetBodyActive(bool isActive)
    {
        sceneLimitsBodyGO.SetActive(isActive);
    }

    public void SetDetailsToggleAsOpen()
    {
        detailsToggleBtn.sprite = openMenuSprite;
    }

    public void SetDetailsToggleAsClose()
    {
        detailsToggleBtn.sprite = closeMenuSprite;
    }

    public void SetTitleText(string text)
    {
        titleTxt.text = text;
    }

    public void SetLeftDescText(string text)
    {
        leftDescTxt.text = text;
    }

    public void SetRightDescText(string text)
    {
        rightDescTxt.text = text;
    }
}
