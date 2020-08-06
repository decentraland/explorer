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

    private float timer;
    private RectTransform thisRT;
    private RectTransform parentRT;
    private HotSceneData hotSceneData;

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

    void SetSceneData(HotSceneData sceneData)
    {
        sceneName.text = sceneData.mapInfo.name;
        coordinates.text = $"{sceneData.crowdInfo.baseCoords.x},{sceneData.crowdInfo.baseCoords.y}";
        creatorName.text = sceneData.mapInfo.owner;
        description.text = sceneData.mapInfo.description;
        thumbnail.sprite = sceneData.thumbnail;
        hotSceneData = sceneData;
    }

    void Awake()
    {
        thisRT = (RectTransform)transform;
        parentRT = (RectTransform)transform.parent;

        this.enabled = false;
        gameObject.SetActive(false);

        hoverArea.OnPointerEnter += OnPointerEnter;
        hoverArea.OnPointerExit += OnPointerExit;

        SceneCellView.OnInfoButtonPointerEnter += OnInfoButtonPointerEnter;
        SceneCellView.OnInfoButtonPointerExit += OnInfoButtonPointerExit;

        showHideAnimator.OnWillFinishHide += OnHidden;
    }

    void OnDestroy()
    {
        hoverArea.OnPointerEnter -= OnPointerEnter;
        hoverArea.OnPointerExit -= OnPointerExit;

        SceneCellView.OnInfoButtonPointerEnter -= OnInfoButtonPointerEnter;
        SceneCellView.OnInfoButtonPointerExit -= OnInfoButtonPointerExit;

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
        hotSceneData = null;
    }

    void OnInfoButtonPointerEnter(HotSceneData sceneData)
    {
        if (sceneData == hotSceneData)
            return;


        SetSceneData(sceneData);

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
}
