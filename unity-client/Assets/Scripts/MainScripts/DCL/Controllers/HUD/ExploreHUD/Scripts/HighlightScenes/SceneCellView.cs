using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using TMPro;
using System;

internal class SceneCellView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static event Action<HotSceneData> OnInfoButtonPointerEnter;
    public static event Action OnInfoButtonPointerExit;

    [SerializeField] Image thumbnail;
    [SerializeField] TextMeshProUGUI sceneName;
    [SerializeField] GameObject crowdCountContainer;
    [SerializeField] TextMeshProUGUI crowdCount;
    [SerializeField] Button jumpIn;
    [SerializeField] ShowHideAnimator jumpInButtonAnimator;
    [SerializeField] GameObject friendsContainer;
    [SerializeField] GameObject eventsContainer;
    [SerializeField] UIHoverCallback sceneInfoButton;
    [SerializeField] GameObject loadingSpinner;

    HotSceneData hotSceneData;

    public void Setup(HotSceneData sceneData)
    {
        hotSceneData = sceneData;
        sceneName.text = sceneData.mapInfo.name;
        crowdCount.text = sceneData.crowdInfo.usersTotalCount.ToString();

        SetThumbnailSprite(sceneData.thumbnail);
    }

    public void SetThumbnailSprite(Sprite sprite)
    {
        thumbnail.sprite = sprite;
        if (sprite == null)
        {
            loadingSpinner.SetActive(true);
        }
        else
        {
            loadingSpinner.SetActive(false);
        }
    }

    void Awake()
    {
        jumpInButtonAnimator.gameObject.SetActive(false);
        //crowdCountContainer.gameObject.SetActive(false);
        eventsContainer.gameObject.SetActive(false);

        sceneInfoButton.OnPointerEnter += () => OnInfoButtonPointerEnter?.Invoke(hotSceneData);
        sceneInfoButton.OnPointerExit += () => OnInfoButtonPointerExit?.Invoke();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (!jumpInButtonAnimator.gameObject.activeSelf)
        {
            jumpInButtonAnimator.gameObject.SetActive(true);
        }
        jumpInButtonAnimator.Show();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        jumpInButtonAnimator.Hide();
    }
}
