using UnityEngine;
using TMPro;
using System;

internal class HotSceneCellView : BaseSceneCellView, ICrowdDataView
{
    public static event Action<HotSceneCellView> OnInfoButtonPointerEnter;
    public static event Action OnInfoButtonPointerExit;

    [SerializeField] GameObject crowdCountContainer;
    [SerializeField] TextMeshProUGUI crowdCount;
    [SerializeField] ShowHideAnimator jumpInButtonAnimator;
    [SerializeField] GameObject friendsContainer;
    [SerializeField] GameObject eventsContainer;
    [SerializeField] UIHoverCallback sceneInfoButton;
    [SerializeField] UIHoverCallback hoverAreaCallback;

    HotScenesController.HotSceneInfo crowdInfo;

    protected override void Awake()
    {
        base.Awake();
        sceneInfoButton.OnPointerEnter += () => OnInfoButtonPointerEnter?.Invoke(this);
        sceneInfoButton.OnPointerExit += () => OnInfoButtonPointerExit?.Invoke();

        hoverAreaCallback.OnPointerEnter += () =>
        {
            jumpInButtonAnimator.gameObject.SetActive(true);
            jumpInButtonAnimator.Show();
        };
        hoverAreaCallback.OnPointerExit += () => jumpInButtonAnimator.Hide();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        jumpInButtonAnimator.gameObject.SetActive(false);
    }

    void ICrowdDataView.SetCrowdInfo(HotScenesController.HotSceneInfo info)
    {
        crowdInfo = info;
        crowdCount.text = info.usersTotalCount.ToString();
    }

    public override void JumpInPressed()
    {
        JumpIn(crowdInfo.baseCoords, crowdInfo.realms[0].serverName, crowdInfo.realms[0].layer);
    }
}
