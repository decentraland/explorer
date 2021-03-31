using System;
using DCL;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

internal class LandElementView : MonoBehaviour, IDisposable
{
    const string SIZE_TEXT_FORMAT = "{0} LAND";

    public event Action<string> OnJumpInPressed; 
    public event Action<string> OnEditorPressed; 
    public event Action<string> OnSettingsPressed; 
        
    [SerializeField] private Texture2D defaultThumbnail;
    [SerializeField] private RawImageFillParent thumbnail;
    [SerializeField] private TextMeshProUGUI landName;
    [SerializeField] private TextMeshProUGUI landCoords;
    [SerializeField] private TextMeshProUGUI landSize;
    [SerializeField] private GameObject landSizeGO;
    [SerializeField] private GameObject roleOwner;
    [SerializeField] private GameObject roleOperator;
    [SerializeField] private Button buttonSettings;
    [SerializeField] private Button buttonJumpIn;
    [SerializeField] private Button buttonEditor;

    public LandSearchInfo searchInfo { get; } = new LandSearchInfo();

    private bool isDestroyed = false;
    private string landId;
    private string thumbnailUrl;
    private AssetPromise_Texture thumbnailPromise;

    private void Awake()
    {
        buttonSettings.onClick.AddListener(()=> OnSettingsPressed?.Invoke(landId));
        buttonJumpIn.onClick.AddListener(()=> OnJumpInPressed?.Invoke(landId));
        buttonEditor.onClick.AddListener(()=> OnEditorPressed?.Invoke(landId));
    }

    private void OnDestroy()
    {
        isDestroyed = true;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetId(string id)
    {
        landId = id;
        searchInfo.id = id;
    }

    public string GetId()
    {
        return landId;
    }

    public void SetName(string name)
    {
        landName.text = name;
        searchInfo.SetName(name);
    }

    public void SetCoords(int x, int y)
    {
        landCoords.text = $"{x},{y}";
    }

    public void SetSize(int size)
    {
        landSizeGO.SetActive(size > 1);
        landSize.text = string.Format(SIZE_TEXT_FORMAT, size);
        searchInfo.SetSize(size);
    }

    public void SetRole(bool isOwner)
    {
        roleOwner.SetActive(isOwner);
        roleOperator.SetActive(!isOwner);
        searchInfo.SetRole(isOwner);
    }

    public Transform GetParent()
    {
        return transform.parent;
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    public void SetThumbnail(string url)
    {
        if (url == thumbnailUrl)
            return;
        
        var prevPromise = thumbnailPromise;

        if (string.IsNullOrEmpty(url))
        {
            SetThumbnail(defaultThumbnail);
        }
        else
        {
            thumbnailPromise = new AssetPromise_Texture(url);
            thumbnailPromise.OnSuccessEvent += asset => SetThumbnail(asset.texture);
            thumbnailPromise.OnFailEvent += asset => SetThumbnail(defaultThumbnail);
            AssetPromiseKeeper_Texture.i.Keep(thumbnailPromise);
        }
        
        if (prevPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(prevPromise);
        }
    }

    public void SetThumbnail(Texture thumbnailTexture)
    {
        thumbnail.texture = thumbnailTexture;
    }

    public void Dispose()
    {
        if (!isDestroyed)
        {
            Destroy(gameObject);
        }
        if (thumbnailPromise != null)
        {
            AssetPromiseKeeper_Texture.i.Forget(thumbnailPromise);
            thumbnailPromise = null;
        }
    }

}
