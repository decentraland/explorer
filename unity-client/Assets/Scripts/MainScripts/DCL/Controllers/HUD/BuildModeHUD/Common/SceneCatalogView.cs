using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneCatalogView : MonoBehaviour
{
    public event System.Action OnSceneCatalogBack;

    [Header("Prefab References")]
    [SerializeField] internal TextMeshProUGUI catalogTitleTxt;
    [SerializeField] internal CatalogAssetPackListView catalogAssetPackListView;
    [SerializeField] internal CatalogGroupListView catalogGroupListView;
    [SerializeField] internal TMP_InputField searchInputField;
    [SerializeField] internal Toggle categoryToggle;
    [SerializeField] internal Toggle favoritesToggle;
    [SerializeField] internal Toggle assetPackToggle;

    [Header("Catalog RectTransforms")]
    [SerializeField] internal RectTransform panelRT;
    [SerializeField] internal RectTransform headerRT;
    [SerializeField] internal RectTransform searchBarRT;
    [SerializeField] internal RectTransform assetPackRT;
    [SerializeField] internal RectTransform categoryRT;

    [Header("MinSize Catalog RectTransforms")]
    [SerializeField] internal RectTransform panelMinSizeRT;
    [SerializeField] internal RectTransform headerMinSizeRT;
    [SerializeField] internal RectTransform searchBarMinSizeRT;
    [SerializeField] internal RectTransform assetPackMinSizeRT;

    [Header("MaxSize Catalog RectTransforms")]
    [SerializeField] internal RectTransform panelMaxSizeRT;
    [SerializeField] internal RectTransform headerMaxSizeRT;
    [SerializeField] internal RectTransform searchBarMaxSizeRT;
    [SerializeField] internal RectTransform assetPackMaxSizeRT;

    private bool isCatalogExpanded = false;

    // TODO (Santi): Called from imspector!
    public void ToggleCatalogExpanse()
    {
        if(isCatalogExpanded)
        {
            BuilderInWorldUtils.CopyRectTransform(panelRT, panelMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(headerRT, headerMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(searchBarRT, searchBarMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(assetPackRT, assetPackMinSizeRT);
            BuilderInWorldUtils.CopyRectTransform(categoryRT, assetPackMinSizeRT);
        }
        else
        {
            BuilderInWorldUtils.CopyRectTransform(panelRT, panelMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(headerRT, headerMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(searchBarRT, searchBarMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(assetPackRT, assetPackMaxSizeRT);
            BuilderInWorldUtils.CopyRectTransform(categoryRT, assetPackMaxSizeRT);
        }

        isCatalogExpanded = !isCatalogExpanded;
    }

    // TODO (Santi): Called from imspector!
    public void Back()
    {
        OnSceneCatalogBack?.Invoke();
    }

    public void SetCatalogTitle(string text)
    {
        catalogTitleTxt.text = text;
    }

    public bool IsCatalogOpen()
    {
        return gameObject.activeSelf;
    }

    public void CloseCatalog()
    {
        if(gameObject.activeSelf)
            StartCoroutine(CloseCatalogAfterOneFrame());
    }

    private IEnumerator CloseCatalogAfterOneFrame()
    {
        yield return null;
        gameObject.SetActive(false);
    }
}
