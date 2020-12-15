using DCL.Interface;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VoiceChatDisabledTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private const string URL_CLAIM_NAME = "https://avatars.decentraland.org/claim";

    private static readonly int showDisabledTooltipAnimation = Animator.StringToHash("ShowDisabledTooltip");
    private static readonly int hideDisabledTooltipAnimation = Animator.StringToHash("HideDisabledTooltip");
    private static readonly int hoverTooltipAnimation = Animator.StringToHash("Hover");

    [SerializeField] private Animator tooltipAnimator;
    [SerializeField] private GameObject disabledBySceneGameObject;
    [SerializeField] private GameObject disabledByFeatureGameObject;
    [SerializeField] private Button claimNameButton;

    private void Awake()
    {
        claimNameButton.onClick.AddListener(()=>
        {
            WebInterface.OpenURL(URL_CLAIM_NAME);
        });
    }

    public void ShowTooltip()
    {
        tooltipAnimator.SetTrigger(hideDisabledTooltipAnimation);
        tooltipAnimator.SetTrigger(showDisabledTooltipAnimation);
    }

    public void HideTooltip()
    {
        tooltipAnimator.SetTrigger(hideDisabledTooltipAnimation);
    }

    public void SetLockedBySceneMode()
    {
        disabledBySceneGameObject.SetActive(true);
        disabledByFeatureGameObject.SetActive(false);
    }

    public void SetLockedByFeatureMode()
    {
        disabledBySceneGameObject.SetActive(false);
        disabledByFeatureGameObject.SetActive(true);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        tooltipAnimator.SetBool(hoverTooltipAnimation, true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        tooltipAnimator.SetBool(hoverTooltipAnimation, false);
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        HideTooltip();
    }
}
