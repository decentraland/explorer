using DCL;
using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

internal interface ISceneCardView : IDisposable
{
    event Action<ISceneData> OnJumpInPressed;
    event Action<ISceneData> OnEditorPressed;
    event Action<ISceneData, ISceneCardView> OnContextMenuPressed;
    ISceneData sceneData { get; }
    ISearchInfo searchInfo { get; }
    Vector3 contextMenuButtonPosition { get; }
    void Setup(ISceneData sceneData);
    void SetParent(Transform parent);
    void SetToDefaultParent();
    void ConfigureDefaultParent(Transform parent);
    void SetName(string name);
    void SetCoords(Vector2Int coords);
    void SetSize(Vector2Int size);
    void SetThumbnail(string thumbnailUrl);
    void SetThumbnail(Texture2D thumbnailTexture);
    void SetDeployed(bool deployed);
    void SetUserRole(bool isOwner, bool isOperator, bool isContributor);
    void SetActive(bool active);
    void SetSiblingIndex(int index);
}

internal class SceneCardView : MonoBehaviour, ISceneCardView
{
    public event Action<ISceneData> OnJumpInPressed;
    public event Action<ISceneData> OnEditorPressed;
    public event Action<ISceneData, ISceneCardView> OnContextMenuPressed;

    [SerializeField] private Texture2D defaultThumbnail;
    [Space]

    [SerializeField] private RawImageFillParent thumbnail;
    [SerializeField] private TextMeshProUGUI sceneName;
    [Space]

    [SerializeField] internal GameObject coordsContainer;
    [SerializeField] private TextMeshProUGUI coordsText;
    [Space]

    [SerializeField] internal GameObject sizeContainer;
    [SerializeField] private TextMeshProUGUI sizeText;
    [Space]

    [SerializeField] internal Button jumpInButton;
    [SerializeField] internal Button editorButton;
    [SerializeField] internal Button contextMenuButton;
    [Space]

    [SerializeField] internal GameObject roleOwnerGO;
    [SerializeField] internal GameObject roleOperatorGO;
    [SerializeField] internal GameObject roleContributorGO;

    ISearchInfo ISceneCardView.searchInfo { get; } = new SearchInfo();
    ISceneData ISceneCardView.sceneData => sceneData;
    Vector3 ISceneCardView.contextMenuButtonPosition => contextMenuButton.transform.position;
    
    private ISceneData sceneData;
    private AssetPromise_Texture thumbnailPromise;
    private bool isDestroyed = false;
    private Transform defaultParent;

    private void Awake()
    {
        jumpInButton.onClick.AddListener(()=> OnJumpInPressed?.Invoke(sceneData));
        editorButton.onClick.AddListener(()=> OnEditorPressed?.Invoke(sceneData));
        contextMenuButton.onClick.AddListener(()=> OnContextMenuPressed?.Invoke(sceneData, this));
    }

    void ISceneCardView.Setup(ISceneData sceneData)
    {
        this.sceneData = sceneData;
        
        ISceneCardView thisView = this;
        thisView.SetThumbnail(sceneData.thumbnailUrl);
        thisView.SetName(sceneData.name);
        thisView.SetCoords(sceneData.coords);
        thisView.SetSize(sceneData.size);
        thisView.SetDeployed(sceneData.isDeployed);
        thisView.SetUserRole(sceneData.isOwner, sceneData.isOperator, sceneData.isContributor);
        
        thisView.searchInfo.SetId(sceneData.id);
    }

    void ISceneCardView.SetParent(Transform parent)
    {
        if (transform.parent == parent)
            return;

        transform.SetParent(parent);
        transform.ResetLocalTRS();
    }

    void ISceneCardView.SetName(string name)
    {
        sceneName.text = name;
        ((ISceneCardView)this).searchInfo.SetName(name);
    }

    void ISceneCardView.SetCoords(Vector2Int coords)
    {
        string coordStr = $"{coords.x},{coords.y}";
        coordsText.text = coordStr;
        ((ISceneCardView)this).searchInfo.SetCoords(coordStr);
    }

    void ISceneCardView.SetSize(Vector2Int size)
    {
        sizeText.text = $"{size.x},{size.y}m";
        ((ISceneCardView)this).searchInfo.SetSize(size.x * size.y);
    }

    void ISceneCardView.SetThumbnail(string thumbnailUrl)
    {
        if (thumbnailPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            thumbnailPromise = null;
        }

        if (string.IsNullOrEmpty(thumbnailUrl))
        {
            ((ISceneCardView)this).SetThumbnail((Texture2D) null);
            return;
        }

        thumbnailPromise = new AssetPromise_Texture(thumbnailUrl);
        thumbnailPromise.OnSuccessEvent += texture => ((ISceneCardView)this).SetThumbnail(texture.texture);
        thumbnailPromise.OnFailEvent += texture => ((ISceneCardView)this).SetThumbnail((Texture2D) null);

        AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
    }

    void ISceneCardView.SetThumbnail(Texture2D thumbnailTexture)
    {
        thumbnail.texture = thumbnailTexture ?? defaultThumbnail;
    }

    void ISceneCardView.SetDeployed(bool deployed)
    {
        coordsContainer.SetActive(deployed);
        sizeContainer.SetActive(!deployed);
        jumpInButton.gameObject.SetActive(deployed);
    }

    void ISceneCardView.SetUserRole(bool isOwner, bool isOperator, bool isContributor)
    {
        roleOwnerGO.SetActive(false);
        roleOperatorGO.SetActive(false);
        roleContributorGO.SetActive(false);
        ((ISceneCardView)this).searchInfo.SetRole(isOwner);

        if (isOwner)
        {
            roleOwnerGO.SetActive(true);
        }
        else if (isOperator)
        {
            roleOperatorGO.SetActive(true);
        }
        else if (isContributor)
        {
            roleContributorGO.SetActive(true);
        }
    }

    void ISceneCardView.SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    void ISceneCardView.SetSiblingIndex(int index)
    {
        transform.SetSiblingIndex(index);
    }
    void ISceneCardView.SetToDefaultParent()
    {
        transform.SetParent(defaultParent);
    }

    void ISceneCardView.ConfigureDefaultParent(Transform parent)
    {
        defaultParent = parent;
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
        isDestroyed = true;
    }
}
