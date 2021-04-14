using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface IBuilderInWorldLoadingView
{
    event System.Action OnCancelLoading;

    void Show();
    void Hide(bool forzeHidding = false);
    void StartTipsCarousel();
    void StopTipsCarousel();
    void CancelLoading(DCLAction_Trigger action);
    void SetPercentage(float newValue);
}

public class BuilderInWorldLoadingView : MonoBehaviour, IBuilderInWorldLoadingView
{
    private const string VIEW_PATH = "BuilderInWorldLoadingView";

    [Header("Loading Tips Config")]
    [SerializeField] internal BuilderInWorldLoadingTip loadingTipPrefab;
    [SerializeField] internal RectTransform loadingTipsContainer;
    [SerializeField] internal ScrollRect loadingTipsScroll;
    [SerializeField] internal List<BuilderInWorldLoadingTipModel> loadingTips;
    [SerializeField] internal float timeBetweenTips = 3f;
    [SerializeField] internal AnimationCurve animationTipsCurve;
    [SerializeField] internal float animationTipsTransitionTime = 1f;

    [Header("Other Config")]
    [SerializeField] internal InputAction_Trigger cancelLoadingInputAction;
    [SerializeField] internal float minVisibilityTime = 1.5f;
    [SerializeField] internal LoadingBar loadingBar;

    public event System.Action OnCancelLoading;

    internal Coroutine tipsCoroutine;
    internal Coroutine hideCoroutine;
    internal float showTime = 0f;
    internal int currentTipIndex = 0;

    internal static BuilderInWorldLoadingView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<BuilderInWorldLoadingView>();
        view.gameObject.name = "_BuilderInWorldLoadingView";

        return view;
    }

    private void Awake() { CreateLoadingTips(); }

    private void OnEnable() { cancelLoadingInputAction.OnTriggered += CancelLoading; }

    private void OnDisable() { cancelLoadingInputAction.OnTriggered -= CancelLoading; }

    private void CreateLoadingTips()
    {
        RectTransform tipsContainerRectTranform = loadingTipsContainer;
        float tipsContainerHorizontalOffset = 0;

        for (int i = 0; i < loadingTips.Count; i++)
        {
            BuilderInWorldLoadingTip newTip = Instantiate(loadingTipPrefab, loadingTipsContainer);
            newTip.Configure(loadingTips[i]);

            if (i > 0)
            {
                tipsContainerHorizontalOffset += ((RectTransform)newTip.transform).rect.size.x;
            }
        }

        tipsContainerRectTranform.offsetMax = new Vector2(tipsContainerHorizontalOffset, tipsContainerRectTranform.offsetMax.y);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        showTime = Time.realtimeSinceStartup;

        if (loadingTips.Count > 0)
        {
            StartTipsCarousel();
        }
    }

    public void Hide(bool forzeHidding = false)
    {
        if (hideCoroutine != null)
            CoroutineStarter.Stop(hideCoroutine);

        hideCoroutine = CoroutineStarter.Start(TryToHideCoroutine(forzeHidding));
    }

    public void StartTipsCarousel()
    {
        StopTipsCarousel();
        tipsCoroutine = CoroutineStarter.Start(RunTipsCarouselCoroutine());
    }

    public void StopTipsCarousel()
    {
        if (tipsCoroutine == null)
            return;

        CoroutineStarter.Stop(tipsCoroutine);
        tipsCoroutine = null;
    }

    internal IEnumerator TryToHideCoroutine(bool forzeHidding)
    {
        while (!forzeHidding && (Time.realtimeSinceStartup - showTime) < minVisibilityTime)
        {
            yield return null;
        }

        StopTipsCarousel();
        gameObject.SetActive(false);
    }

    internal IEnumerator RunTipsCarouselCoroutine()
    {
        currentTipIndex = 0;

        while (true)
        {
            yield return new WaitForSeconds(timeBetweenTips);
            yield return RunTipsAnimationCoroutine();

            currentTipIndex++;
            if (currentTipIndex >= loadingTips.Count - 1)
            {
                currentTipIndex = 0;
                loadingTipsScroll.horizontalNormalizedPosition = 0f;

                // Moving the last tip game object to the first position in the hierarchy, we make the carousel cyclical.
                loadingTipsContainer.GetChild(loadingTipsContainer.childCount - 1).SetAsFirstSibling();
            }
        }
    }

    internal IEnumerator RunTipsAnimationCoroutine()
    {
        float currentAnimationTime = 0f;
        float initialNormalizedPos = loadingTipsScroll.horizontalNormalizedPosition;
        float finalNormalizedPos = initialNormalizedPos + (1f / (loadingTips.Count - 1));

        while (currentAnimationTime <= animationTipsTransitionTime)
        {
            loadingTipsScroll.horizontalNormalizedPosition = Mathf.Lerp(
                initialNormalizedPos,
                finalNormalizedPos,
                animationTipsCurve.Evaluate(currentAnimationTime / animationTipsTransitionTime));

            currentAnimationTime += Time.deltaTime;

            yield return null;
        }
    }

    public void CancelLoading(DCLAction_Trigger action)
    {
        Hide();
        OnCancelLoading?.Invoke();
    }

    public void SetPercentage(float newValue) { loadingBar.SetPercentage(newValue); }
}