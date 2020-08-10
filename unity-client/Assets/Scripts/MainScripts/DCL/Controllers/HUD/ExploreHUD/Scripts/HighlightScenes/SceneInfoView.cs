using UnityEngine;
using UnityEngine.UI;
using TMPro;

internal class SceneInfoView : MonoBehaviour
{
    [SerializeField] float idleTime;
    [SerializeField] Image thumbnail;
    [SerializeField] TextMeshProUGUI sceneName;
    [SerializeField] TextMeshProUGUI coordinates;
    [SerializeField] TextMeshProUGUI creatorName;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] Button jumpIn;
    [SerializeField] ShowHideAnimator showHideAnimator;
    [SerializeField] UIHoverCallback hoverArea;
    [SerializeField] GameObject loadingSpinner;

    private float timer;
    private RectTransform thisRT;
    private RectTransform parentRT;
    private HotSceneCellView hotSceneView;

    public void Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        showHideAnimator.Show();
        this.enabled = false;
    }

    public void Show(Vector2 position)
    {
        thisRT.anchoredPosition = position;
        Show();
    }

    public void Hide()
    {
        Hide(false);
    }

    public void Hide(bool instant)
    {
        if (instant)
        {
            showHideAnimator.Hide(true);
        }
        else
        {
            timer = idleTime;
            this.enabled = true;
        }
    }

    void SetSceneView(HotSceneCellView sceneView)
    {
        if (hotSceneView)
        {
            hotSceneView.OnThumbnailFetched -= SetThumbnail;
        }

        hotSceneView = sceneView;

        SetMapInfoData(sceneView);

        thumbnail.sprite = sceneView.GetThumbnail();
        bool hasThumbnail = thumbnail.sprite != null;
        loadingSpinner.SetActive(!hasThumbnail);
        if (!hasThumbnail)
        {
            sceneView.OnThumbnailFetched += SetThumbnail;
        }
    }

    void SetMapInfoData(IMapDataView mapInfoView)
    {
        MinimapMetadata.MinimapSceneInfo mapInfo = mapInfoView.GetMinimapSceneInfo();
        sceneName.text = mapInfo.name;
        coordinates.text = $"{mapInfoView.GetBaseCoord().x},{mapInfoView.GetBaseCoord().y}";
        creatorName.text = mapInfo.owner;
        description.text = mapInfo.description;
    }

    void SetThumbnail(Sprite thumbnailSprite)
    {
        thumbnail.sprite = thumbnailSprite;
        loadingSpinner.SetActive(thumbnailSprite != null);
    }

    void Awake()
    {
        thisRT = (RectTransform)transform;
        parentRT = (RectTransform)transform.parent;

        this.enabled = false;
        gameObject.SetActive(false);

        jumpIn.onClick.AddListener(() =>
        {
            if (hotSceneView)
            {
                hotSceneView.JumpInPressed();
            }
        });

        hoverArea.OnPointerEnter += OnPointerEnter;
        hoverArea.OnPointerExit += OnPointerExit;

        HotSceneCellView.OnInfoButtonPointerEnter += OnInfoButtonPointerEnter;
        HotSceneCellView.OnInfoButtonPointerExit += OnInfoButtonPointerExit;

        BaseSceneCellView.OnJumpIn += OnJumpIn;

        showHideAnimator.OnWillFinishHide += OnHidden;
    }

    void OnDestroy()
    {
        hoverArea.OnPointerEnter -= OnPointerEnter;
        hoverArea.OnPointerExit -= OnPointerExit;

        HotSceneCellView.OnInfoButtonPointerEnter -= OnInfoButtonPointerEnter;
        HotSceneCellView.OnInfoButtonPointerExit -= OnInfoButtonPointerExit;

        BaseSceneCellView.OnJumpIn -= OnJumpIn;

        showHideAnimator.OnWillFinishHide -= OnHidden;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            showHideAnimator.Hide();
            this.enabled = false;
        }
    }

    void OnHidden(ShowHideAnimator animator)
    {
        hotSceneView = null;
    }

    void OnInfoButtonPointerEnter(HotSceneCellView sceneView)
    {
        if (sceneView == hotSceneView)
            return;


        SetSceneView(sceneView);

        Vector2 localpoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, Input.mousePosition, null, out localpoint))
        {
            Show(localpoint);
        }
    }

    void OnInfoButtonPointerExit()
    {
        Hide();
    }

    void OnPointerEnter()
    {
        Show();
    }

    void OnPointerExit()
    {
        Hide();
    }

    void OnJumpIn(Vector2Int coords, string serverName, string layerName)
    {
        gameObject.SetActive(false);
    }
}
