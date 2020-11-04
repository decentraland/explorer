using DCL;
using UnityEngine;
using DCL.Interface;
using System.Collections;
using System;

public class ProfileHUDController : IHUD
{
    [Serializable]
    public struct Configuration
    {
        public bool enableManaCounter;
    }

    private const string URL_CLAIM_NAME = "https://avatars.decentraland.org/claim";
    private const string URL_MANA_INFO = "https://docs.decentraland.org/examples/get-a-wallet";
    private const string URL_MANA_PURCHASE = "https://market.decentraland.org/settings";
    private const float FETCH_MANA_INTERVAL = 60;

    internal ProfileHUDView view;
    internal ManaCounterView manaCounterView;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private IMouseCatcher mouseCatcher;
    private Coroutine fetchManaIntervalRoutine = null;

    public ProfileHUDController()
    {
        mouseCatcher = InitialSceneReferences.i?.mouseCatcher;

        view = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("ProfileHUD")).GetComponent<ProfileHUDView>();
        view.name = "_ProfileHUD";

        view.buttonLogOut.onClick.AddListener(WebInterface.LogOut);
        view.buttonClaimName.onClick.AddListener(()=> WebInterface.OpenURL(URL_CLAIM_NAME));

        manaCounterView = view.GetComponentInChildren<ManaCounterView>();
        if (manaCounterView)
        {
            manaCounterView.buttonManaInfo.onPointerDown += OnManaInfoPressed;
            manaCounterView.buttonManaPurchase.onPointerDown += OnManaPurchasePressed;
        }
        SetManaCounterVisibility(false);

        ownUserProfile.OnUpdate += OnProfileUpdated;
        if (mouseCatcher != null) mouseCatcher.OnMouseLock += OnMouseLocked;
    }

    public void SetVisibility(bool visible)
    {
        view?.SetVisibility(visible);

        if (visible && fetchManaIntervalRoutine == null)
        {
            fetchManaIntervalRoutine = CoroutineStarter.Start(ManaIntervalRoutine());
        }
        else if (!visible && fetchManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchManaIntervalRoutine);
            fetchManaIntervalRoutine = null;
        }
    }

    public void Dispose()
    {
        if (fetchManaIntervalRoutine != null)
        {
            CoroutineStarter.Stop(fetchManaIntervalRoutine);
            fetchManaIntervalRoutine = null;
        }

        if (manaCounterView != null)
        {
            manaCounterView.buttonManaInfo.onPointerDown -= OnManaInfoPressed;
            manaCounterView.buttonManaPurchase.onPointerDown -= OnManaPurchasePressed;
        }

        if (view)
        {
            GameObject.Destroy(view.gameObject);
        }
        ownUserProfile.OnUpdate -= OnProfileUpdated;
        if (mouseCatcher != null) mouseCatcher.OnMouseLock -= OnMouseLocked;
    }

    void OnProfileUpdated(UserProfile profile)
    {
        view?.SetProfile(profile);
    }

    void OnMouseLocked()
    {
        view.HideMenu();
    }

    void OnManaInfoPressed()
    {
        WebInterface.OpenURL(URL_MANA_INFO);
    }

    void OnManaPurchasePressed()
    {
        WebInterface.OpenURL(URL_MANA_PURCHASE);
    }

    IEnumerator ManaIntervalRoutine()
    {
        while (true)
        {
            WebInterface.FetchBalanceOfMANA();
            yield return WaitForSecondsCache.Get(FETCH_MANA_INTERVAL);
        }
    }

    public void SetManaBalance(string balance)
    {
        manaCounterView?.SetBalance(balance);
    }

    public void SetManaCounterVisibility(bool visible)
    {
        manaCounterView?.gameObject.SetActive(visible);
    }
}
