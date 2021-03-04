using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface ISceneLimitsView
{
    Color lfColor { get; }
    Color mfColor { get; }
    Color hfColor { get; }
    Image[] limitUsageFillsImages { get; }

    bool isBodyActived { get; }

    event Action OnToggleSceneLimitsInfo;

    void SetBodyActive(bool isActive);
    void SetDetailsToggleAsClose();
    void SetDetailsToggleAsOpen();
    void SetLeftDescText(string text);
    void SetRightDescText(string text);
    void SetTitleText(string text);
    void SetUpdateCallback(UnityAction call);
    void ToggleSceneLimitsInfo();
}

public class SceneLimitsView : MonoBehaviour, ISceneLimitsView
{
    public Color lfColor => lowFillColor;
    public Color mfColor => mediumFillColor;
    public Color hfColor => highFillColor;
    public Image[] limitUsageFillsImages => limitUsageFillsImgs;

    public event Action OnToggleSceneLimitsInfo;

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
    [SerializeField] internal TextMeshProUGUI leftDescTxt;
    [SerializeField] internal TextMeshProUGUI rightDescTxt;
    [SerializeField] internal Image[] limitUsageFillsImgs;
    [SerializeField] internal Button toggleButton;

    [Header("Input Actions")]
    [SerializeField] internal InputAction_Trigger toggleSceneInfoInputAction;

    public bool isBodyActived => sceneLimitsBodyGO.activeSelf;

    internal UnityAction updateInfoAction;
    private const int FRAMES_BETWEEN_UPDATES = 15;

    private const string VIEW_PATH = "GodMode/Inspector/SceneLimitsView";

    internal static SceneLimitsView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<SceneLimitsView>();
        view.gameObject.name = "_SceneLimitsView";

        return view;
    }

    private void Awake()
    {
        toggleSceneInfoInputAction.OnTriggered += (action) => ToggleSceneLimitsInfo();
        toggleButton.onClick.AddListener(ToggleSceneLimitsInfo);
    }

    private void Update()
    {
        if (Time.frameCount % FRAMES_BETWEEN_UPDATES == 0)
            updateInfoAction();
    }

    private void OnDestroy()
    {
        toggleSceneInfoInputAction.OnTriggered -= (action) => ToggleSceneLimitsInfo();
        toggleButton.onClick.RemoveListener(ToggleSceneLimitsInfo);
    }

    public void SetUpdateCallback(UnityAction call)
    {
        updateInfoAction = call;
    }

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
