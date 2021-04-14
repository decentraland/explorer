using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public interface IBuilderInWorldLoadingView
{
    event System.Action OnCancelLoading;

    void Show(bool showTips = true);
    void Hide(bool forzeHidding = false);
    void StartTipsCarousel();
    void StopTipsCarousel();
    void CancelLoading(DCLAction_Trigger action);
    void SetPercentage(float newValue);
}

public class BuilderInWorldLoadingView : MonoBehaviour, IBuilderInWorldLoadingView
{
    private const string VIEW_PATH = "BuilderInWorldLoadingView";

    [SerializeField] internal BuilderInWorldLoadingTip loadingTipItem;
    [SerializeField] internal List<BuilderInWorldLoadingTipModel> loadingTips;
    [SerializeField] internal float timeBetweenTips = 3f;
    [SerializeField] internal InputAction_Trigger cancelLoadingInputAction;
    [SerializeField] internal float minVisibilityTime = 1.5f;
    [SerializeField] internal LoadingBar loadingBar;

    public event System.Action OnCancelLoading;

    internal Coroutine tipsCoroutine;
    internal Coroutine hideCoroutine;
    internal float showTime = 0f;

    internal static BuilderInWorldLoadingView Create()
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<BuilderInWorldLoadingView>();
        view.gameObject.name = "_BuilderInWorldLoadingView";

        return view;
    }

    private void OnEnable() { cancelLoadingInputAction.OnTriggered += CancelLoading; }

    private void OnDisable() { cancelLoadingInputAction.OnTriggered -= CancelLoading; }

    public void Show(bool showTips = true)
    {
        gameObject.SetActive(true);
        showTime = Time.realtimeSinceStartup;

        if (showTips && loadingTips.Count > 0)
        {
            StartTipsCarousel();
        }
        else
        {
            loadingTipItem.Configure(new BuilderInWorldLoadingTipModel
            {
                tipMessage = string.Empty,
                tipImage = null
            });
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
        tipsCoroutine = CoroutineStarter.Start(ShowRandomTipsCoroutine());
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

    internal IEnumerator ShowRandomTipsCoroutine()
    {
        while (true)
        {
            loadingTipItem.Configure(loadingTips[Random.Range(0, loadingTips.Count - 1)]);
            yield return new WaitForSeconds(timeBetweenTips);
        }
    }

    public void CancelLoading(DCLAction_Trigger action)
    {
        Hide();
        OnCancelLoading?.Invoke();
    }

    public void SetPercentage(float newValue) { loadingBar.SetPercentage(newValue); }
}