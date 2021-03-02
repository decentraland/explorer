using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EntityInformationView : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] internal Sprite openMenuSprite;
    [SerializeField] internal Sprite closeMenuSprite;

    [Header("Prefab references")]
    [SerializeField] internal TextMeshProUGUI titleTxt;
    [SerializeField] internal TextMeshProUGUI entityLimitsLeftTxt;
    [SerializeField] internal TextMeshProUGUI entityLimitsRightTxt;
    [SerializeField] internal TMP_InputField nameIF;
    [SerializeField] internal RawImage entitytTumbailImg; 
    [SerializeField] internal AttributeXYZ positionAttribute;
    [SerializeField] internal AttributeXYZ rotationAttribute;
    [SerializeField] internal AttributeXYZ scaleAttribute;
    [SerializeField] internal GameObject detailsGO;
    [SerializeField] internal GameObject basicsGO;
    [SerializeField] internal Image detailsToggleBtn;
    [SerializeField] internal Image basicToggleBtn;
    [SerializeField] internal SmartItemListView smartItemListView;
    [SerializeField] internal Button backButton;
    [SerializeField] internal Button hideCatalogButton;
    [SerializeField] internal Button detailsBackButton;
    [SerializeField] internal Button basicInfoBackButton;

    public event Action<DCLBuilderInWorldEntity, string> OnNameChange;
    public event Action<DCLBuilderInWorldEntity> OnUpdateInfo;
    public event Action OnStartChangingName;
    public event Action OnEndChangingName;
    public event Action OnDisable;

    public DCLBuilderInWorldEntity currentEntity { get; set; }
    public bool isEnable { get; set; } = false;

    private const int FRAMES_BETWEEN_UPDATES = 5;

    private void Awake()
    {
        backButton.onClick.AddListener(Disable);
        hideCatalogButton.onClick.AddListener(Disable);
        detailsBackButton.onClick.AddListener(ToggleDetailsInfo);
        basicInfoBackButton.onClick.AddListener(ToggleBasicInfo);
        nameIF.onEndEdit.AddListener((newName) => ChangeEntityName(newName));
        nameIF.onSelect.AddListener((newName) => StartChangingName());
        nameIF.onDeselect.AddListener((newName) => EndChangingName());
    }

    private void OnDestroy()
    {
        backButton.onClick.RemoveListener(Disable);
        hideCatalogButton.onClick.RemoveListener(Disable);
        detailsBackButton.onClick.RemoveListener(ToggleDetailsInfo);
        basicInfoBackButton.onClick.RemoveListener(ToggleBasicInfo);
        nameIF.onEndEdit.RemoveListener((newName) => ChangeEntityName(newName));
        nameIF.onSelect.RemoveListener((newName) => StartChangingName());
        nameIF.onDeselect.RemoveListener((newName) => EndChangingName());
    }

    private void LateUpdate()
    {
        if (!isEnable)
            return;

        if (currentEntity == null)
            return;

        if (Time.frameCount % FRAMES_BETWEEN_UPDATES == 0)
            OnUpdateInfo?.Invoke(currentEntity);
    }

    public void ToggleDetailsInfo()
    {
        detailsGO.SetActive(!detailsGO.activeSelf);
        detailsToggleBtn.sprite = detailsGO.activeSelf ? openMenuSprite : closeMenuSprite;
    }

    public void ToggleBasicInfo()
    {
        basicsGO.SetActive(!basicsGO.activeSelf);
        basicToggleBtn.sprite = basicsGO.activeSelf ? openMenuSprite : closeMenuSprite;
    }

    public void StartChangingName()
    {
        OnStartChangingName?.Invoke();
    }

    public void EndChangingName()
    {
        OnEndChangingName?.Invoke();
    }

    public void ChangeEntityName(string newName)
    {
        OnNameChange?.Invoke(currentEntity, newName);
    }

    public void Disable()
    {
        OnDisable?.Invoke();
    }

    public void SetEntityThumbnailEnable(bool isEnable)
    {
        entitytTumbailImg.enabled = isEnable;
    }

    public void SetEntityThumbnailTexture(Texture2D texture)
    {
        entitytTumbailImg.texture = texture;
    }

    public void SeTitleText(string text)
    {
        titleTxt.text = text;
    }

    public void SeEntityLimitsLeftText(string text)
    {
        entityLimitsLeftTxt.text = text;
    }

    public void SeEntityLimitsRightText(string text)
    {
        entityLimitsRightTxt.text = text;
    }
}